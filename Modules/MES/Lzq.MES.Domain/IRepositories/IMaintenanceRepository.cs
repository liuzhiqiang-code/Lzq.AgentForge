using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.Enums;
using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.MES.Domain.IRepositories;

/// <summary>
/// 报修单仓储接口
/// </summary>
public interface IRepairOrderRepository : ISqlSugarRepository<RepairOrderEntity>, ITransientDependency
{
    /// <summary>
    /// 获取设备的报修记录
    /// </summary>
    Task<List<RepairOrderEntity>> GetByEquipmentIdAsync(long equipmentId);

    /// <summary>
    /// 根据状态查询报修单
    /// </summary>
    Task<List<RepairOrderEntity>> GetByStatusAsync(RepairStatusEnum status);

    /// <summary>
    /// 获取待派工的报修单
    /// </summary>
    Task<List<RepairOrderEntity>> GetPendingOrdersAsync();

    /// <summary>
    /// 获取报修单统计
    /// </summary>
    Task<(int pendingCount, int inProgressCount, int completedCount)> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate);
}

/// <summary>
/// 保养计划仓储接口
/// </summary>
public interface IMaintenancePlanRepository : ISqlSugarRepository<MaintenancePlanEntity>, ITransientDependency
{
    /// <summary>
    /// 获取设备的保养计划
    /// </summary>
    Task<List<MaintenancePlanEntity>> GetByEquipmentIdAsync(long equipmentId);

    /// <summary>
    /// 获取需要执行的保养计划
    /// </summary>
    Task<List<MaintenancePlanEntity>> GetPendingPlansAsync(DateTime beforeDate);

    /// <summary>
    /// 获取今日保养计划
    /// </summary>
    Task<List<MaintenancePlanEntity>> GetTodayPlansAsync();
}

/// <summary>
/// 保养记录仓储接口
/// </summary>
public interface IMaintenanceRecordRepository : ISqlSugarRepository<MaintenanceRecordEntity>, ITransientDependency
{
    /// <summary>
    /// 获取设备的保养记录
    /// </summary>
    Task<List<MaintenanceRecordEntity>> GetByEquipmentIdAsync(long equipmentId);

    /// <summary>
    /// 获取保养记录统计
    /// </summary>
    Task<int> GetCountAsync(long? equipmentId, DateTime? fromDate, DateTime? toDate);
}
