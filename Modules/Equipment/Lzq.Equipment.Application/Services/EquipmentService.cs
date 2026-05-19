using Microsoft.AspNetCore.Builder;
using Lzq.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Lzq.Equipment.Domain.Enums;
using Lzq.Equipment.Domain.Consts;
using Lzq.Extensions.Redis;
using Mapster;
using NSwag.Annotations;
using SqlSugar;
using Lzq.Equipment.Application.Contracts.Queries;
using Lzq.Equipment.Application.Contracts.Commands;
using Lzq.Equipment.Application.Contracts.IServices;
using Lzq.Equipment.Application.Contracts.Dtos;
using Lzq.Equipment.Domain.Entities;
using Lzq.Equipment.Domain.IRepositories;

namespace Lzq.Equipment.Application.Services;

/// <summary>
/// 设备台账服务
/// </summary>
public class EquipmentService : ServiceBase, IEquipmentService
{
    public EquipmentService() : base("/api/v1/mes/equipment") { }

    private IEquipmentRepository EquipmentRepo => GetRequiredService<IEquipmentRepository>();

    /// <summary>
    /// 设备分页查询
    /// </summary>
    [OpenApiTag("mes/equipment"), OpenApiOperation("设备分页查询", "支持按编号、名称、类型、状态、产线筛选")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<EquipmentViewDto>>> PageAsync([FromBody] EquipmentPageQuery query)
    {
        var expr = EquipmentRepo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Code), e => e.Code!.Contains(query.Code!))
            .WhereIF(!string.IsNullOrEmpty(query.Name), e => e.Name!.Contains(query.Name!))
            .WhereIF(query.EquipmentType.HasValue, e => e.EquipmentType == query.EquipmentType.Value)
            .WhereIF(query.Status.HasValue, e => e.Status == query.Status.Value)
            .WhereIF(query.LineId.HasValue, e => e.LineId == query.LineId.Value)
            .WhereIF(query.ResponsibleId.HasValue, e => e.ResponsibleId == query.ResponsibleId.Value)
            .WhereIF(query.PurchaseDateFrom.HasValue, e => e.PurchaseDate >= query.PurchaseDateFrom.Value)
            .WhereIF(query.PurchaseDateTo.HasValue, e => e.PurchaseDate <= query.PurchaseDateTo.Value)
            .WhereIF(query.CreateTimeFrom.HasValue, e => e.CreationTime >= query.CreateTimeFrom.Value)
            .WhereIF(query.CreateTimeTo.HasValue, e => e.CreationTime <= query.CreateTimeTo.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(e => e.Code)
            .ToPageListAsync(query.Page, query.PageSize, total);

        var dtos = list.Select(e =>
        {
            var dto = e.Map<EquipmentViewDto>();
            dto.EquipmentTypeName = GetEquipmentTypeName(e.EquipmentType);
            dto.StatusName = GetStatusName(e.Status);
            return dto;
        }).ToList();

        return ApiResult.Success(new PagedResponse<EquipmentViewDto>(dtos, total));
    }

    /// <summary>
    /// 获取设备详情
    /// </summary>
    [OpenApiTag("mes/equipment"), OpenApiOperation("获取设备详情", "根据ID获取设备详细信息")]
    [RoutePattern(pattern: "{id}", true)]
    public async Task<ApiResult<EquipmentViewDto>> GetAsync(long id)
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var cacheKey = string.Format(RedisKeys.Get, id);
        
        var dto = await redis.GetOrSetAsync(cacheKey, async () =>
        {
            var entity = await EquipmentRepo.GetByIdAsync(id)
                ?? throw new UserFriendlyException("设备不存在");

            var result = entity.Map<EquipmentViewDto>();
            result.EquipmentTypeName = GetEquipmentTypeName(entity.EquipmentType);
            result.StatusName = GetStatusName(entity.Status);
            return result;
        }, TimeSpan.FromHours(1)); // 缓存1小时

        return ApiResult.Success(dto!);
    }

    /// <summary>
    /// 创建设备
    /// </summary>
    [OpenApiTag("mes/equipment"), OpenApiOperation("创建设备", "创建设备台账")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] EquipmentCreateCommand command)
    {
        var exists = await EquipmentRepo.ExistsByCodeAsync(command.Code);
        if (exists) throw new UserFriendlyException($"设备编号 [{command.Code}] 已存在");

        var entity = command.Map<EquipmentEntity>();
        entity.Status = EquipmentStatusEnum.Normal;
        entity.TotalRunningHours = 0;
        entity.TotalRepairCount = 0;

        await EquipmentRepo.InsertAsync(entity);
        
        // 清除统计缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.Statistics);
        
        return ApiResult.Success(entity.Id);
    }

    /// <summary>
    /// 更新设备
    /// </summary>
    [OpenApiTag("mes/equipment"), OpenApiOperation("更新设备", "更新设备信息")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult<bool>> UpdateAsync([FromBody] EquipmentUpdateCommand command)
    {
        var entity = await EquipmentRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("设备不存在");

        command.Map(entity);
        await EquipmentRepo.UpdateAsync(entity);

        // 清除设备和统计缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(string.Format(RedisKeys.Get, command.Id));
        await redis.RemoveAsync(RedisKeys.Statistics);
        
        return ApiResult.Success(true);
    }

    /// <summary>
    /// 删除设备
    /// </summary>
    [OpenApiTag("mes/equipment"), OpenApiOperation("删除设备", "删除设备台账")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await EquipmentRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("设备不存在");

        await EquipmentRepo.DeleteAsync(entity);

        // 清除设备和统计缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(string.Format(RedisKeys.Get, id));
        await redis.RemoveAsync(RedisKeys.Statistics);
        
        return ApiResult.Success(true);
    }

    /// <summary>
    /// 更新设备状态
    /// </summary>
    [OpenApiTag("mes/equipment"), OpenApiOperation("更新设备状态", "更新设备状态（正常/维修中/停机/报废）")]
    [RoutePattern(pattern: "update-status", true)]
    public async Task<ApiResult<bool>> UpdateStatusAsync([FromBody] EquipmentUpdateStatusCommand command)
    {
        var entity = await EquipmentRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("设备不存在");

        entity.Status = command.Status;
        await EquipmentRepo.UpdateAsync(entity);

        // 清除设备和统计缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(string.Format(RedisKeys.Get, command.Id));
        await redis.RemoveAsync(RedisKeys.Statistics);
        
        return ApiResult.Success(true);
    }

    /// <summary>
    /// 获取设备统计
    /// </summary>
    [OpenApiTag("mes/equipment"), OpenApiOperation("设备统计", "获取设备状态统计")]
    [RoutePattern(pattern: "statistics", true)]
    public async Task<ApiResult<EquipmentStatisticsDto>> GetStatisticsAsync()
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var cacheKey = RedisKeys.Statistics;
        
        var dto = await redis.GetOrSetAsync(cacheKey, async () =>
        {
            var (normalCount, repairCount, stoppedCount) = await EquipmentRepo.GetStatisticsAsync();

            var totalCount = await EquipmentRepo.AsQueryable().CountAsync();
            var productionCount = await EquipmentRepo.AsQueryable()
                .Where(e => e.EquipmentType == EquipmentTypeEnum.Production).CountAsync();
            var testingCount = await EquipmentRepo.AsQueryable()
                .Where(e => e.EquipmentType == EquipmentTypeEnum.Testing).CountAsync();
            var auxiliaryCount = await EquipmentRepo.AsQueryable()
                .Where(e => e.EquipmentType == EquipmentTypeEnum.Auxiliary).CountAsync();

            return new EquipmentStatisticsDto
            {
                TotalCount = totalCount,
                NormalCount = normalCount,
                RepairCount = repairCount,
                StoppedCount = stoppedCount,
                ProductionCount = productionCount,
                TestingCount = testingCount,
                AuxiliaryCount = auxiliaryCount
            };
        }, TimeSpan.FromMinutes(10)); // 统计数据缓存10分钟

        return ApiResult.Success(dto!);
    }

    #region 辅助方法

    private static string GetEquipmentTypeName(EquipmentTypeEnum type) => type switch
    {
        EquipmentTypeEnum.Production => "生产设备",
        EquipmentTypeEnum.Testing => "检测设备",
        EquipmentTypeEnum.Auxiliary => "辅助设备",
        EquipmentTypeEnum.Power => "动力设备",
        EquipmentTypeEnum.Transport => "运输设备",
        _ => "未知"
    };

    private static string GetStatusName(EquipmentStatusEnum status) => status switch
    {
        EquipmentStatusEnum.Normal => "正常",
        EquipmentStatusEnum.UnderRepair => "维修中",
        EquipmentStatusEnum.UnderMaintenance => "保养中",
        EquipmentStatusEnum.Stopped => "停机",
        EquipmentStatusEnum.Scrapped => "报废",
        _ => "未知"
    };

    #endregion
}
