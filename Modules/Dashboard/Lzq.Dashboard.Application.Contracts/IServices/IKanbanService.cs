using Lzq.Dashboard.Application.Contracts.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.Dashboard.Application.Contracts.IServices;

/// <summary>
/// 看板服务接口
/// </summary>
public interface IKanbanService : ITransientDependency
{
    /// <summary>
    /// 获取产量统计汇总
    /// </summary>
    /// <param name="lineId">产线ID（可选）</param>
    Task<ApiResult<ProductionOutputSummaryDto>> GetProductionOutputSummaryAsync(long? lineId = null);

    /// <summary>
    /// 获取不良率趋势汇总
    /// </summary>
    /// <param name="lineId">产线ID（可选）</param>
    Task<ApiResult<DefectRateSummaryDto>> GetDefectRateSummaryAsync(long? lineId = null);

    /// <summary>
    /// 获取工单完成率汇总
    /// </summary>
    /// <param name="lineId">产线ID（可选）</param>
    Task<ApiResult<WorkOrderCompletionSummaryDto>> GetWorkOrderCompletionSummaryAsync(long? lineId = null);

    /// <summary>
    /// 获取设备状态概览
    /// </summary>
    Task<ApiResult<EquipmentStatusOverviewDto>> GetEquipmentStatusOverviewAsync();

    /// <summary>
    /// 获取看板配置列表
    /// </summary>
    /// <param name="configType">配置类型（可选）</param>
    Task<ApiResult<List<DashboardConfigDto>>> GetConfigListAsync(int? configType = null);

    /// <summary>
    /// 获取看板配置
    /// </summary>
    /// <param name="id">配置ID</param>
    Task<ApiResult<DashboardConfigDto?>> GetConfigAsync(long id);

    /// <summary>
    /// 创建看板配置
    /// </summary>
    Task<ApiResult<DashboardConfigDto>> CreateConfigAsync(DashboardConfigDto config);

    /// <summary>
    /// 更新看板配置
    /// </summary>
    Task<ApiResult<DashboardConfigDto>> UpdateConfigAsync(DashboardConfigDto config);

    /// <summary>
    /// 删除看板配置
    /// </summary>
    Task<ApiResult> DeleteConfigAsync(long id);
}
