using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Lzq.MES.Domain.Consts;
using Lzq.Extensions.Redis;
using Lzq.Extensions.SqlSugar.Repository;
using Mapster;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Application.Contracts.IServices.WorkOrder;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Domain.Entities;

namespace Lzq.MES.Application.Services;

/// <summary>
/// 看板服务实现
/// </summary>
[OpenApiTag("dashboard/kanban")]
public class KanbanService : ServiceBase, IKanbanService
{
    public KanbanService() : base("/api/v1/dashboard/kanban")
    {
    }

    #region 服务注入

    private ISqlSugarRepository<DashboardConfigEntity> ConfigRepository => 
        GetRequiredService<ISqlSugarRepository<DashboardConfigEntity>>();

    private IWorkOrderStatisticsService WorkOrderStatisticsService => 
        GetRequiredService<IWorkOrderStatisticsService>();

    private IQCStatisticsService QCStatisticsService => 
        GetRequiredService<IQCStatisticsService>();

    private IEquipmentStatisticsService EquipmentStatisticsService => 
        GetRequiredService<IEquipmentStatisticsService>();

    #endregion

    private ILogger<KanbanService> Logger => GetRequiredService<ILogger<KanbanService>>();

    /// <summary>
    /// 获取产量统计汇总
    /// </summary>
    [OpenApiOperation("获取产量统计汇总", "")]
    [RoutePattern(pattern: "production/summary", true)]
    public async Task<ApiResult<ProductionOutputSummaryDto>> GetProductionOutputSummaryAsync(long? lineId = null)
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var cacheKey = string.Format(RedisKeys.ProductionSummary, lineId ?? 0);
        
        var result = await redis.GetOrSetAsync(cacheKey, async () =>
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            // 从工单统计服务获取今日/本周/本月产量
            var todayOutput = await WorkOrderStatisticsService.GetProductionOutputAsync(today, today.AddDays(1), lineId);
            var weekOutput = await WorkOrderStatisticsService.GetProductionOutputAsync(weekStart, today.AddDays(1), lineId);
            var monthOutput = await WorkOrderStatisticsService.GetProductionOutputAsync(monthStart, today.AddDays(1), lineId);

            // 从工单统计服务获取完成率
            var todayCompletionRate = await WorkOrderStatisticsService.GetCompletionRateAsync(today, today.AddDays(1), lineId);
            var weekCompletionRate = await WorkOrderStatisticsService.GetCompletionRateAsync(weekStart, today.AddDays(1), lineId);
            var monthCompletionRate = await WorkOrderStatisticsService.GetCompletionRateAsync(monthStart, today.AddDays(1), lineId);

            // 获取趋势数据（最近7天）
            var trendList = new List<DailyOutputDto>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var output = await WorkOrderStatisticsService.GetProductionOutputAsync(date, date.AddDays(1), lineId);
                trendList.Add(new DailyOutputDto
                {
                    Date = date,
                    Quantity = output
                });
            }

            return new ProductionOutputSummaryDto
            {
                TodayOutput = todayOutput,
                WeekOutput = weekOutput,
                MonthOutput = monthOutput,
                TodayCompletionRate = todayCompletionRate,
                WeekCompletionRate = weekCompletionRate,
                MonthCompletionRate = monthCompletionRate,
                TrendList = trendList
            };
        }, TimeSpan.FromMinutes(5)); // 看板数据缓存5分钟

        return ApiResult.Success(result!);
    }

    /// <summary>
    /// 获取不良率趋势汇总
    /// </summary>
    [OpenApiOperation("获取不良率趋势汇总", "")]
    [RoutePattern(pattern: "defect/summary", true)]
    public async Task<ApiResult<DefectRateSummaryDto>> GetDefectRateSummaryAsync(long? lineId = null)
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var cacheKey = string.Format(RedisKeys.DefectSummary, lineId ?? 0);
        
        var result = await redis.GetOrSetAsync(cacheKey, async () =>
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            // 从质检统计服务获取今日/本周/本月不良率
            var todayStats = await QCStatisticsService.GetDefectRateStatisticsAsync(today, today.AddDays(1), lineId);
            var weekStats = await QCStatisticsService.GetDefectRateStatisticsAsync(weekStart, today.AddDays(1), lineId);
            var monthStats = await QCStatisticsService.GetDefectRateStatisticsAsync(monthStart, today.AddDays(1), lineId);

            // 获取趋势数据（最近7天）
            var trendList = new List<DailyDefectRateDto>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var stats = await QCStatisticsService.GetDefectRateStatisticsAsync(date, date.AddDays(1), lineId);
                trendList.Add(new DailyDefectRateDto
                {
                    Date = date,
                    QualifiedRate = stats.QualifiedRate,
                    DefectCount = stats.UnqualifiedQty
                });
            }

            return new DefectRateSummaryDto
            {
                TodayQualifiedRate = todayStats.QualifiedRate,
                WeekQualifiedRate = weekStats.QualifiedRate,
                MonthQualifiedRate = monthStats.QualifiedRate,
                TrendList = trendList
            };
        }, TimeSpan.FromMinutes(5));

        return ApiResult.Success(result!);
    }

    /// <summary>
    /// 获取工单完成率汇总
    /// </summary>
    [OpenApiOperation("获取工单完成率汇总", "")]
    [RoutePattern(pattern: "workorder/summary", true)]
    public async Task<ApiResult<WorkOrderCompletionSummaryDto>> GetWorkOrderCompletionSummaryAsync(long? lineId = null)
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var cacheKey = string.Format(RedisKeys.WorkOrderSummary, lineId ?? 0);
        
        var result = await redis.GetOrSetAsync(cacheKey, async () =>
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var todayStats = await WorkOrderStatisticsService.GetStatisticsAsync(today, today.AddDays(1), lineId);
            var weekStats = await WorkOrderStatisticsService.GetStatisticsAsync(weekStart, today.AddDays(1), lineId);
            var monthStats = await WorkOrderStatisticsService.GetStatisticsAsync(monthStart, today.AddDays(1), lineId);

            return new WorkOrderCompletionSummaryDto
            {
                TodayCompletionRate = todayStats.CompletionRate,
                WeekCompletionRate = weekStats.CompletionRate,
                MonthCompletionRate = monthStats.CompletionRate,
                TodayStats = new WorkOrderCompletionDto
                {
                    TotalCount = todayStats.TotalCount,
                    CompletedCount = todayStats.CompletedCount,
                    InProgressCount = todayStats.InProgressCount,
                    PendingCount = todayStats.PendingCount,
                    CompletionRate = todayStats.CompletionRate
                },
                WeekStats = new WorkOrderCompletionDto
                {
                    TotalCount = weekStats.TotalCount,
                    CompletedCount = weekStats.CompletedCount,
                    InProgressCount = weekStats.InProgressCount,
                    PendingCount = weekStats.PendingCount,
                    CompletionRate = weekStats.CompletionRate
                },
                MonthStats = new WorkOrderCompletionDto
                {
                    TotalCount = monthStats.TotalCount,
                    CompletedCount = monthStats.CompletedCount,
                    InProgressCount = monthStats.InProgressCount,
                    PendingCount = monthStats.PendingCount,
                    CompletionRate = monthStats.CompletionRate
                }
            };
        }, TimeSpan.FromMinutes(5));

        return ApiResult.Success(result!);
    }

    /// <summary>
    /// 获取设备状态概览
    /// </summary>
    [OpenApiOperation("获取设备状态概览", "")]
    [RoutePattern(pattern: "equipment/overview", true)]
    public async Task<ApiResult<EquipmentStatusOverviewDto>> GetEquipmentStatusOverviewAsync()
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var cacheKey = RedisKeys.EquipmentOverview;
        
        var result = await redis.GetOrSetAsync(cacheKey, async () =>
        {
            // 从设备统计服务获取概览
            var overview = await EquipmentStatisticsService.GetStatusOverviewAsync();
            var lineStats = await EquipmentStatisticsService.GetStatusByLineAsync();

            return new EquipmentStatusOverviewDto
            {
                TotalCount = overview.TotalCount,
                NormalCount = overview.NormalCount,
                UnderRepairCount = overview.UnderRepairCount,
                UnderMaintenanceCount = overview.UnderMaintenanceCount,
                StoppedCount = overview.StoppedCount,
                NormalRate = overview.NormalRate,
                StatusList = lineStats.Map<List<EquipmentStatusItemDto>>()
            };
        }, TimeSpan.FromMinutes(5));

        return ApiResult.Success(result!);
    }

    /// <summary>
    /// 获取看板配置列表
    /// </summary>
    [OpenApiOperation("获取看板配置列表", "")]
    [RoutePattern(pattern: "config/list", true)]
    public async Task<ApiResult<List<DashboardConfigDto>>> GetConfigListAsync(int? configType = null)
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var cacheKey = string.Format(RedisKeys.ConfigList, configType ?? 0);
        
        var list = await redis.GetOrSetAsync(cacheKey, async () =>
        {
            var query = ConfigRepository.AsQueryable();
            
            if (configType.HasValue)
            {
                query = query.Where(c => c.ConfigType == configType.Value);
            }

            var result = await query.ToListAsync();
            return result.Map<List<DashboardConfigDto>>();
        }, TimeSpan.FromHours(1)); // 配置缓存1小时

        return ApiResult.Success(list ?? new List<DashboardConfigDto>());
    }

    /// <summary>
    /// 获取看板配置
    /// </summary>
    [OpenApiOperation("获取看板配置", "")]
    [RoutePattern(pattern: "config/{id}", true)]
    public async Task<ApiResult<DashboardConfigDto?>> GetConfigAsync(long id)
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var cacheKey = string.Format(RedisKeys.Config, id);
        
        var config = await redis.GetOrSetAsync(cacheKey, async () =>
        {
            return await ConfigRepository.GetByIdAsync(id);
        }, TimeSpan.FromHours(1)); // 配置缓存1小时

        return ApiResult.Success(config?.Map<DashboardConfigDto>());
    }

    /// <summary>
    /// 创建看板配置
    /// </summary>
    [OpenApiOperation("创建看板配置", "")]
    [RoutePattern(pattern: "config/create", true)]
    public async Task<ApiResult<DashboardConfigDto>> CreateConfigAsync([FromBody] DashboardConfigDto dto)
    {
        var entity = dto.Map<DashboardConfigEntity>();
        await ConfigRepository.InsertAsync(entity);
        
        // 清除配置列表缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(string.Format(RedisKeys.ConfigList, dto.ConfigType));
        await redis.RemoveAsync(RedisKeys.ConfigListAll); // 清除全部配置缓存
        
        return ApiResult.Success(entity.Map<DashboardConfigDto>());
    }

    /// <summary>
    /// 更新看板配置
    /// </summary>
    [OpenApiOperation("更新看板配置", "")]
    [RoutePattern(pattern: "config/update", true)]
    public async Task<ApiResult<DashboardConfigDto>> UpdateConfigAsync([FromBody] DashboardConfigDto dto)
    {
        var entity = await ConfigRepository.GetByIdAsync(dto.Id);
        if (entity == null)
        {
            throw new UserFriendlyException("配置不存在");
        }

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.ConfigType = dto.ConfigType;
        entity.RefreshInterval = dto.RefreshInterval;
        entity.CacheTtl = dto.CacheTtl;
        entity.ConfigJson = dto.ConfigJson;
        entity.IsEnabled = dto.IsEnabled;
        entity.Remark = dto.Remark;

        await ConfigRepository.UpdateAsync(entity);
        
        // 清除相关缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(string.Format(RedisKeys.Config, dto.Id));
        await redis.RemoveAsync(string.Format(RedisKeys.ConfigList, dto.ConfigType));
        await redis.RemoveAsync(RedisKeys.ConfigListAll); // 清除全部配置缓存
        
        return ApiResult.Success(entity.Map<DashboardConfigDto>());
    }

    /// <summary>
    /// 删除看板配置
    /// </summary>
    [OpenApiOperation("删除看板配置", "")]
    [RoutePattern(pattern: "config/delete/{id}", true)]
    public async Task<ApiResult> DeleteConfigAsync(long id)
    {
        var entity = await ConfigRepository.GetByIdAsync(id);
        var configType = entity?.ConfigType;
        
        await ConfigRepository.DeleteByIdAsync(id);
        
        // 清除相关缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(string.Format(RedisKeys.Config, id));
        if (configType.HasValue)
            await redis.RemoveAsync(string.Format(RedisKeys.ConfigList, configType.Value));
        await redis.RemoveAsync(RedisKeys.ConfigListAll); // 清除全部配置缓存
        
        return ApiResult.Success();
    }
}
