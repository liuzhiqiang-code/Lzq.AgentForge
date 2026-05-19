using Lzq.Equipment.Application.Contracts.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.Equipment.Application.Contracts.IServices;

/// <summary>
/// 设备统计服务接口（供Dashboard模块使用）
/// </summary>
public interface IEquipmentStatisticsService : ITransientDependency
{
    /// <summary>
    /// 获取设备状态概览
    /// </summary>
    /// <returns>设备状态概览</returns>
    Task<EquipmentStatusOverviewResultDto> GetStatusOverviewAsync();

    /// <summary>
    /// 获取按产线分组的设备状态统计
    /// </summary>
    /// <returns>产线设备状态列表</returns>
    Task<List<EquipmentStatusByLineResultDto>> GetStatusByLineAsync();
}
