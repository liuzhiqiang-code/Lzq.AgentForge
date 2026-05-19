using Lzq.WorkOrder.Application.Contracts.WorkOrder.Dto;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.WorkOrder.Application.Contracts.IServices.WorkOrder;

/// <summary>
/// 工单统计服务接口（供Dashboard模块使用）
/// </summary>
public interface IWorkOrderStatisticsService : ITransientDependency
{
    /// <summary>
    /// 获取指定时间范围和产线的工单统计
    /// </summary>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="lineId">产线ID（可选）</param>
    /// <returns>工单统计结果</returns>
    Task<WorkOrderStatisticsResultDto> GetStatisticsAsync(DateTime start, DateTime end, long? lineId = null);

    /// <summary>
    /// 获取产量统计（从报工记录汇总）
    /// </summary>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="lineId">产线ID（可选）</param>
    /// <returns>合格产量</returns>
    Task<int> GetProductionOutputAsync(DateTime start, DateTime end, long? lineId = null);

    /// <summary>
    /// 获取工单完成率
    /// </summary>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="lineId">产线ID（可选）</param>
    /// <returns>完成率百分比</returns>
    Task<decimal> GetCompletionRateAsync(DateTime start, DateTime end, long? lineId = null);
}
