using Microsoft.AspNetCore.Builder;
using Lzq.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Lzq.QA.Domain.Enums;
using Lzq.QA.Domain.Consts;
using Lzq.Extensions.Redis;
using Mapster;
using NSwag.Annotations;
using SqlSugar;
using Lzq.QA.Domain.Entities;
using Lzq.QA.Domain.IRepositories;
using Lzq.QA.Application.Contracts.IServices;
using Lzq.QA.Application.Contracts.Commands;
using Lzq.QA.Application.Contracts.Dtos;
using Lzq.QA.Application.Contracts.Queries;

namespace Lzq.QA.Application.Services;

/// <summary>
/// 质检单服务（支持IQC/PQC/OQC）
/// </summary>
public class QCOrderService : ServiceBase, IQCOrderService
{
    public QCOrderService() : base("/api/v1/mes/qc-order") { }

    private IQCOrderRepository QCOrderRepo => GetRequiredService<IQCOrderRepository>();
    private IQCOrderItemRepository QCOrderItemRepo => GetRequiredService<IQCOrderItemRepository>();

    #region 查询

    /// <summary>
    /// 质检单分页查询
    /// </summary>
    [OpenApiTag("mes/qc-order"), OpenApiOperation("质检单分页查询", "支持按编号、类型、供应商、产品、状态、时间范围筛选")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<QCOrderViewDto>>> PageAsync([FromBody] QCOrderPageQuery query)
    {
        var expr = QCOrderRepo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Code), q => q.Code!.Contains(query.Code!))
            .WhereIF(query.QCType.HasValue, q => q.QCType == query.QCType.Value)
            .WhereIF(!string.IsNullOrEmpty(query.RefCode), q => q.RefCode!.Contains(query.RefCode!))
            .WhereIF(query.SupplierId.HasValue, q => q.SupplierId == query.SupplierId.Value)
            .WhereIF(!string.IsNullOrEmpty(query.SupplierName), q => q.SupplierName!.Contains(query.SupplierName!))
            .WhereIF(query.ProductId.HasValue, q => q.ProductId == query.ProductId.Value)
            .WhereIF(!string.IsNullOrEmpty(query.ProductName), q => q.ProductName!.Contains(query.ProductName!))
            .WhereIF(query.Status.HasValue, q => q.Status == query.Status.Value)
            .WhereIF(query.InspectorId.HasValue, q => q.InspectorId == query.InspectorId.Value)
            .WhereIF(query.InspectDateFrom.HasValue, q => q.InspectDate >= query.InspectDateFrom.Value)
            .WhereIF(query.InspectDateTo.HasValue, q => q.InspectDate <= query.InspectDateTo.Value)
            .WhereIF(query.CreateTimeFrom.HasValue, q => q.CreationTime >= query.CreateTimeFrom.Value)
            .WhereIF(query.CreateTimeTo.HasValue, q => q.CreationTime <= query.CreateTimeTo.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(q => q.CreationTime, OrderByType.Desc)
            .ToPageListAsync(query.Page, query.PageSize, total);

        var dtos = list.Select(q =>
        {
            var dto = q.Map<QCOrderViewDto>();
            dto.QCTypeName = GetQCTypeName(q.QCType);
            dto.StatusName = GetStatusName(q.Status);
            return dto;
        }).ToList();

        return ApiResult.Success(new PagedResponse<QCOrderViewDto>(dtos, total));
    }

    /// <summary>
    /// 获取质检单详情
    /// </summary>
    [OpenApiTag("mes/qc-order"), OpenApiOperation("获取质检单详情", "根据ID获取质检单详细信息")]
    [RoutePattern(pattern: "{id}", true)]
    public async Task<ApiResult<QCOrderViewDto>> GetAsync(long id)
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var cacheKey = string.Format(RedisKeys.Get, id);
        
        var dto = await redis.GetOrSetAsync(cacheKey, async () =>
        {
            var entity = await QCOrderRepo.GetByIdAsync(id)
                ?? throw new UserFriendlyException("质检单不存在");

            var result = entity.Map<QCOrderViewDto>();
            result.QCTypeName = GetQCTypeName(entity.QCType);
            result.StatusName = GetStatusName(entity.Status);

            // 获取检验明细
            var items = await QCOrderItemRepo.GetByQCOrderIdAsync(id);
            result.Items = items.Select(i =>
            {
                var itemDto = i.Map<QCOrderItemViewDto>();
                itemDto.ItemTypeName = GetItemTypeName(i.ItemType);
                itemDto.ResultName = GetResultName(i.Result);
                return itemDto;
            }).ToList();

            return result;
        }, TimeSpan.FromMinutes(30)); // 质检单详情缓存30分钟

        return ApiResult.Success(dto!);
    }

    /// <summary>
    /// 获取质检单的检验明细
    /// </summary>
    [OpenApiTag("mes/qc-order"), OpenApiOperation("获取检验明细", "获取指定质检单的所有检验明细")]
    [RoutePattern(pattern: "{id}/items", true)]
    public async Task<ApiResult<List<QCOrderItemViewDto>>> GetItemsAsync(long qcOrderId)
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var cacheKey = string.Format(RedisKeys.Items, qcOrderId);
        
        var dtos = await redis.GetOrSetAsync(cacheKey, async () =>
        {
            var items = await QCOrderItemRepo.GetByQCOrderIdAsync(qcOrderId);
            var result = items.Select(i =>
            {
                var dto = i.Map<QCOrderItemViewDto>();
                dto.ItemTypeName = GetItemTypeName(i.ItemType);
                dto.ResultName = GetResultName(i.Result);
                return dto;
            }).ToList();

            return result;
        }, TimeSpan.FromMinutes(30)); // 检验明细缓存30分钟

        return ApiResult.Success(dtos ?? new List<QCOrderItemViewDto>());
    }

    #endregion

    #region CRUD

    /// <summary>
    /// 创建质检单
    /// </summary>
    [OpenApiTag("mes/qc-order"), OpenApiOperation("创建质检单", "新建质检单，编号自动生成")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] QCOrderCreateCommand command)
    {
        // IQC必须选择供应商
        if (command.QCType == QCTypeEnum.IQC && !command.SupplierId.HasValue)
            throw new UserFriendlyException("来料检验必须选择供应商");

        var entity = command.Map<QCOrderEntity>();
        entity.Code = await GenerateCodeAsync(command.QCType);
        entity.Status = QCOrderStatusEnum.Pending;
        entity.InspectDate = DateTime.Now;
        entity.QualifiedQty = 0;
        entity.UnqualifiedQty = 0;

        await QCOrderRepo.InsertAsync(entity);

        // 如果有检验明细，一并插入
        if (command.Items != null && command.Items.Any())
        {
            foreach (var item in command.Items)
            {
                var itemEntity = item.Map<QCOrderItemEntity>();
                itemEntity.QCOrderId = entity.Id;
                await QCOrderItemRepo.InsertAsync(itemEntity);
            }
        }

        return ApiResult.Success(entity.Id);
    }

    /// <summary>
    /// 更新质检单
    /// </summary>
    [OpenApiTag("mes/qc-order"), OpenApiOperation("更新质检单", "更新质检单信息，只能更新待检验状态的质检单")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult<bool>> UpdateAsync([FromBody] QCOrderUpdateCommand command)
    {
        var entity = await QCOrderRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("质检单不存在");

        if (entity.Status != QCOrderStatusEnum.Pending)
            throw new UserFriendlyException("只能更新待检验状态的质检单");

        command.Map(entity);
        await QCOrderRepo.UpdateAsync(entity);
        
        // 清除缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(string.Format(RedisKeys.Get, command.Id));
        await redis.RemoveAsync(string.Format(RedisKeys.Items, command.Id));
        
        return ApiResult.Success(true);
    }

    /// <summary>
    /// 删除质检单
    /// </summary>
    [OpenApiTag("mes/qc-order"), OpenApiOperation("删除质检单", "删除质检单及其检验明细，只能删除待检验状态的质检单")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await QCOrderRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("质检单不存在");

        if (entity.Status != QCOrderStatusEnum.Pending)
            throw new UserFriendlyException("只能删除待检验状态的质检单");

        // 删除检验明细
        await QCOrderItemRepo.DeleteByQCOrderIdAsync(id);
        await QCOrderRepo.DeleteAsync(entity);
        
        // 清除缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(string.Format(RedisKeys.Get, id));
        await redis.RemoveAsync(string.Format(RedisKeys.Items, id));
        
        return ApiResult.Success(true);
    }

    #endregion

    #region 检验流程
    /// <summary>
    /// 提交检验结果
    /// </summary>
    [OpenApiTag("mes/qc-order"), OpenApiOperation("提交检验结果", "提交检验明细数据")]
    [RoutePattern(pattern: "submit-inspect", true)]
    public async Task<ApiResult<bool>> SubmitInspectAsync([FromBody] QCOrderSubmitInspectCommand command)
    {
        var entity = await QCOrderRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("质检单不存在");

        if (entity.Status != QCOrderStatusEnum.Pending && entity.Status != QCOrderStatusEnum.InProgress)
            throw new UserFriendlyException("只能对待检验或检验中的质检单提交检验结果");

        // 删除旧的检验明细
        await QCOrderItemRepo.DeleteByQCOrderIdAsync(command.Id);

        // 插入新的检验明细
        int totalQualified = 0;
        int totalUnqualified = 0;

        foreach (var item in command.Items)
        {
            var itemEntity = item.Map<QCOrderItemEntity>();
            itemEntity.QCOrderId = entity.Id;
            await QCOrderItemRepo.InsertAsync(itemEntity);

            totalQualified += item.QualifiedQty;
            totalUnqualified += item.UnqualifiedQty;
        }

        // 更新质检单状态和数量
        entity.Status = QCOrderStatusEnum.InProgress;
        entity.QualifiedQty = totalQualified;
        entity.UnqualifiedQty = totalUnqualified;
        await QCOrderRepo.UpdateAsync(entity);

        // 清除缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(string.Format(RedisKeys.Get, command.Id));
        await redis.RemoveAsync(string.Format(RedisKeys.Items, command.Id));
        
        return ApiResult.Success(true);
    }

    /// <summary>
    /// 判定质检单
    /// </summary>
    [OpenApiTag("mes/qc-order"), OpenApiOperation("判定质检单", "对质检单进行最终判定（合格/不合格）")]
    [RoutePattern(pattern: "judge", true)]
    public async Task<ApiResult<bool>> JudgeAsync([FromBody] QCOrderJudgeCommand command)
    {
        var entity = await QCOrderRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("质检单不存在");

        if (entity.Status != QCOrderStatusEnum.InProgress)
            throw new UserFriendlyException("只能判定检验中的质检单");

        entity.Status = command.Result;
        entity.QualifiedQty = command.QualifiedQty;
        entity.UnqualifiedQty = command.UnqualifiedQty;
        entity.Conclusion = command.Conclusion;
        entity.CompletedTime = DateTime.Now;
        if (!string.IsNullOrEmpty(command.Remark))
        {
            entity.Remark = string.IsNullOrEmpty(entity.Remark)
                ? command.Remark
                : $"{entity.Remark}\n{command.Remark}";
        }

        await QCOrderRepo.UpdateAsync(entity);

        // 清除缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(string.Format(RedisKeys.Get, command.Id));
        await redis.RemoveAsync(string.Format(RedisKeys.Items, command.Id));
        
        return ApiResult.Success(true);
    }

    /// <summary>
    /// 取消质检单
    /// </summary>
    [OpenApiTag("mes/qc-order"), OpenApiOperation("取消质检单", "取消质检单")]
    [RoutePattern(pattern: "cancel", true)]
    public async Task<ApiResult<bool>> CancelAsync([FromBody] QCOrderCancelCommand command)
    {
        var entity = await QCOrderRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("质检单不存在");

        if (entity.Status == QCOrderStatusEnum.Processed)
            throw new UserFriendlyException("已处理的质检单不能取消");

        entity.Status = QCOrderStatusEnum.Cancelled;
        if (!string.IsNullOrEmpty(command.Reason))
        {
            entity.Remark = string.IsNullOrEmpty(entity.Remark)
                ? $"取消原因：{command.Reason}"
                : $"{entity.Remark}\n取消原因：{command.Reason}";
        }

        await QCOrderRepo.UpdateAsync(entity);

        // 清除缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(string.Format(RedisKeys.Get, command.Id));
        await redis.RemoveAsync(string.Format(RedisKeys.Items, command.Id));
        
        return ApiResult.Success(true);
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 生成质检单编号
    /// </summary>
    private async Task<string> GenerateCodeAsync(QCTypeEnum qcType)
    {
        var prefix = qcType switch
        {
            QCTypeEnum.IQC => "IQC",
            QCTypeEnum.PQC => "PQC",
            QCTypeEnum.OQC => "OQC",
            _ => "QC"
        };

        var date = DateTime.Now.ToString("yyyyMMdd");
        var count = await QCOrderRepo.AsQueryable()
            .Where(q => q.Code.StartsWith($"{prefix}{date}"))
            .CountAsync();

        return $"{prefix}{date}{(count + 1):D4}";
    }

    private static string GetQCTypeName(QCTypeEnum qcType) => qcType switch
    {
        QCTypeEnum.IQC => "来料检验",
        QCTypeEnum.PQC => "过程检验",
        QCTypeEnum.OQC => "出货检验",
        _ => "未知"
    };

    private static string GetStatusName(QCOrderStatusEnum status) => status switch
    {
        QCOrderStatusEnum.Pending => "待检验",
        QCOrderStatusEnum.InProgress => "检验中",
        QCOrderStatusEnum.Qualified => "合格",
        QCOrderStatusEnum.Unqualified => "不合格",
        QCOrderStatusEnum.Processed => "已处理",
        QCOrderStatusEnum.Cancelled => "已取消",
        _ => "未知"
    };

    private static string GetItemTypeName(int itemType) => itemType switch
    {
        1 => "外观",
        2 => "尺寸",
        3 => "功能",
        4 => "性能",
        5 => "包装",
        6 => "其他",
        _ => "未知"
    };

    private static string GetResultName(QCResultEnum result) => result switch
    {
        QCResultEnum.Pass => "合格",
        QCResultEnum.Fail => "不合格",
        QCResultEnum.AcceptWithRestriction => "让步接收",
        _ => "未知"
    };

    #endregion
}
