using Microsoft.AspNetCore.Builder;
using Lzq.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Lzq.QA.Domain.Enums;
using Mapster;
using NSwag.Annotations;
using SqlSugar;
using Lzq.QA.Domain.Entities;
using Lzq.QA.Domain.IRepositories;
using Lzq.QA.Application.Contracts.Commands;
using Lzq.QA.Application.Contracts.Queries;
using Lzq.QA.Application.Contracts.Dtos;
using Lzq.QA.Application.Contracts.IServices;

namespace Lzq.QA.Application.Services;

/// <summary>
/// 不良品服务
/// </summary>
public class DefectRecordService : ServiceBase, IDefectRecordService
{
    public DefectRecordService() : base("/api/v1/mes/defect") { }

    private IDefectRecordRepository DefectRepo => GetRequiredService<IDefectRecordRepository>();

    #region 查询

    /// <summary>
    /// 不良品分页查询
    /// </summary>
    [OpenApiTag("mes/defect"), OpenApiOperation("不良品分页查询", "支持按质检单、工单、产品、状态、处理方式、时间范围筛选")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<DefectRecordViewDto>>> PageAsync([FromBody] DefectRecordPageQuery query)
    {
        var expr = DefectRepo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.QCOrderCode), d => d.QCOrderCode!.Contains(query.QCOrderCode!))
            .WhereIF(!string.IsNullOrEmpty(query.WorkOrderCode), d => d.WorkOrderCode!.Contains(query.WorkOrderCode!))
            .WhereIF(!string.IsNullOrEmpty(query.ProductName), d => d.ProductName!.Contains(query.ProductName!))
            .WhereIF(!string.IsNullOrEmpty(query.BatchNo), d => d.BatchNo!.Contains(query.BatchNo!))
            .WhereIF(!string.IsNullOrEmpty(query.DefectCode), d => d.DefectCode!.Contains(query.DefectCode!))
            .WhereIF(query.Status.HasValue, d => d.Status == query.Status.Value)
            .WhereIF(query.HandlingType.HasValue, d => d.HandlingType == query.HandlingType.Value)
            .WhereIF(query.CreateTimeFrom.HasValue, d => d.CreationTime >= query.CreateTimeFrom.Value)
            .WhereIF(query.CreateTimeTo.HasValue, d => d.CreationTime <= query.CreateTimeTo.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(d => d.Status, OrderByType.Asc)
            .OrderByDescending(d => d.CreationTime)
            .ToPageListAsync(query.Page, query.PageSize, total);

        var dtos = list.Select(d =>
        {
            var dto = d.Map<DefectRecordViewDto>();
            dto.StatusName = GetStatusName(d.Status);
            dto.HandlingTypeName = d.HandlingType.HasValue ? GetHandlingTypeName(d.HandlingType.Value) : null;
            return dto;
        }).ToList();

        return ApiResult.Success(new PagedResponse<DefectRecordViewDto>(dtos, total));
    }

    /// <summary>
    /// 获取不良品详情
    /// </summary>
    [OpenApiTag("mes/defect"), OpenApiOperation("获取不良品详情", "根据ID获取不良品详细信息")]
    [RoutePattern(pattern: "{id}", true)]
    public async Task<ApiResult<DefectRecordViewDto>> GetAsync(long id)
    {
        var entity = await DefectRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("不良品记录不存在");

        var dto = entity.Map<DefectRecordViewDto>();
        dto.StatusName = GetStatusName(entity.Status);
        dto.HandlingTypeName = entity.HandlingType.HasValue ? GetHandlingTypeName(entity.HandlingType.Value) : null;

        return ApiResult.Success(dto);
    }

    /// <summary>
    /// 获取质检单关联的不良品记录
    /// </summary>
    [OpenApiTag("mes/defect"), OpenApiOperation("获取质检单的不良品", "获取指定质检单关联的所有不良品记录")]
    [RoutePattern(pattern: "by-qc-order/{qcOrderId}", true)]
    public async Task<ApiResult<List<DefectRecordViewDto>>> GetByQCOrderIdAsync(long qcOrderId)
    {
        var list = await DefectRepo.GetByQCOrderIdAsync(qcOrderId);
        var dtos = list.Select(d =>
        {
            var dto = d.Map<DefectRecordViewDto>();
            dto.StatusName = GetStatusName(d.Status);
            dto.HandlingTypeName = d.HandlingType.HasValue ? GetHandlingTypeName(d.HandlingType.Value) : null;
            return dto;
        }).ToList();

        return ApiResult.Success(dtos);
    }

    /// <summary>
    /// 获取不良品统计
    /// </summary>
    [OpenApiTag("mes/defect"), OpenApiOperation("不良品统计", "统计不良品的处理情况")]
    [RoutePattern(pattern: "statistics", true)]
    public async Task<ApiResult<DefectStatisticsDto>> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate)
    {
        var (reworkCount, scrapCount, downgradeCount, returnCount) = await DefectRepo.GetDefectStatisticsAsync(fromDate, toDate);

        var total = await DefectRepo.AsQueryable()
            .WhereIF(fromDate.HasValue, d => d.CreationTime >= fromDate.Value)
            .WhereIF(toDate.HasValue, d => d.CreationTime <= toDate.Value)
            .CountAsync();

        var pendingCount = await DefectRepo.AsQueryable()
            .Where(d => d.Status == DefectStatusEnum.Pending)
            .WhereIF(fromDate.HasValue, d => d.CreationTime >= fromDate.Value)
            .WhereIF(toDate.HasValue, d => d.CreationTime <= toDate.Value)
            .CountAsync();

        var processingCount = await DefectRepo.AsQueryable()
            .Where(d => d.Status == DefectStatusEnum.Processing)
            .WhereIF(fromDate.HasValue, d => d.CreationTime >= fromDate.Value)
            .WhereIF(toDate.HasValue, d => d.CreationTime <= toDate.Value)
            .CountAsync();

        var processedCount = await DefectRepo.AsQueryable()
            .Where(d => d.Status == DefectStatusEnum.Processed)
            .WhereIF(fromDate.HasValue, d => d.CreationTime >= fromDate.Value)
            .WhereIF(toDate.HasValue, d => d.CreationTime <= toDate.Value)
            .CountAsync();

        var dto = new DefectStatisticsDto
        {
            TotalDefectCount = total,
            ReworkCount = reworkCount,
            ScrapCount = scrapCount,
            DowngradeCount = downgradeCount,
            ReturnCount = returnCount,
            PendingCount = pendingCount,
            ProcessingCount = processingCount,
            ProcessedCount = processedCount
        };

        return ApiResult.Success(dto);
    }

    #endregion

    #region CRUD

    /// <summary>
    /// 创建不良品记录
    /// </summary>
    [OpenApiTag("mes/defect"), OpenApiOperation("创建不良品记录", "手动创建不良品记录")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] DefectRecordCreateCommand command)
    {
        var entity = command.Map<DefectRecordEntity>();
        entity.Status = DefectStatusEnum.Pending;

        await DefectRepo.InsertAsync(entity);

        return ApiResult.Success(entity.Id);
    }

    /// <summary>
    /// 删除不良品记录
    /// </summary>
    [OpenApiTag("mes/defect"), OpenApiOperation("删除不良品记录", "删除不良品记录，只能删除待处理状态的记录")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await DefectRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("不良品记录不存在");

        if (entity.Status != DefectStatusEnum.Pending)
            throw new UserFriendlyException("只能删除待处理状态的不良品记录");

        await DefectRepo.DeleteAsync(entity);

        return ApiResult.Success(true);
    }

    #endregion

    #region 处理流程

    /// <summary>
    /// 处理不良品
    /// </summary>
    [OpenApiTag("mes/defect"), OpenApiOperation("处理不良品", "对不良品进行处理（返工/报废/降级/退货/特采）")]
    [RoutePattern(pattern: "handle", true)]
    public async Task<ApiResult<bool>> HandleAsync([FromBody] DefectRecordHandleCommand command)
    {
        var entity = await DefectRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("不良品记录不存在");

        if (entity.Status == DefectStatusEnum.Processed)
            throw new UserFriendlyException("已处理的不良品不能再次处理");

        // 如果需要评审，处理方式必须为有效值
        if (entity.NeedReview && command.HandlingType == default(DefectHandlingEnum))
            throw new UserFriendlyException("该不良品需要评审后才能处理");

        entity.Status = DefectStatusEnum.Processed;
        entity.HandlingType = command.HandlingType;
        entity.HandlingRemark = command.HandlingRemark;
        entity.HandlingTime = DateTime.Now;
        if (!string.IsNullOrEmpty(command.Remark))
        {
            entity.Remark = string.IsNullOrEmpty(entity.Remark)
                ? command.Remark
                : $"{entity.Remark}\n{command.Remark}";
        }

        await DefectRepo.UpdateAsync(entity);

        return ApiResult.Success(true);
    }

    /// <summary>
    /// 评审不良品
    /// </summary>
    [OpenApiTag("mes/defect"), OpenApiOperation("评审不良品", "对需要评审的不良品进行评审决定处理方式")]
    [RoutePattern(pattern: "review", true)]
    public async Task<ApiResult<bool>> ReviewAsync([FromBody] DefectRecordReviewCommand command)
    {
        var entity = await DefectRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("不良品记录不存在");

        if (!entity.NeedReview)
            throw new UserFriendlyException("该不良品不需要评审");

        entity.ReviewResult = command.ReviewResult;
        entity.ReviewTime = DateTime.Now;
        if (!string.IsNullOrEmpty(command.ReviewRemark))
        {
            entity.Remark = string.IsNullOrEmpty(entity.Remark)
                ? $"评审说明：{command.ReviewRemark}"
                : $"{entity.Remark}\n评审说明：{command.ReviewRemark}";
        }

        // 如果评审时指定了处理方式，直接处理
        if (command.HandlingType.HasValue)
        {
            entity.Status = DefectStatusEnum.Processed;
            entity.HandlingType = command.HandlingType;
            entity.HandlingTime = DateTime.Now;
        }
        else
        {
            entity.Status = DefectStatusEnum.Processing;
        }

        await DefectRepo.UpdateAsync(entity);

        return ApiResult.Success(true);
    }

    #endregion

    #region 辅助方法

    private static string GetStatusName(DefectStatusEnum? status) => status switch
    {
        DefectStatusEnum.Pending => "待处理",
        DefectStatusEnum.Processing => "处理中",
        DefectStatusEnum.Processed => "已处理",
        _ => "未知"
    };

    private static string GetHandlingTypeName(DefectHandlingEnum handlingType) => handlingType switch
    {
        DefectHandlingEnum.Rework => "返工",
        DefectHandlingEnum.Scrap => "报废",
        DefectHandlingEnum.Downgrade => "降级使用",
        DefectHandlingEnum.Return => "退货",
        DefectHandlingEnum.AcceptSpecial => "特采",
        _ => "未知"
    };

    #endregion
}
