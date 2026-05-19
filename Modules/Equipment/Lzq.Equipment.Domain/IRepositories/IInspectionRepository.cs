using Lzq.Equipment.Domain.Entities;
using Lzq.Equipment.Domain.Enums;
using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.Equipment.Domain.IRepositories;

/// <summary>
/// 点检计划仓储接口
/// </summary>
public interface IInspectionPlanRepository : ISqlSugarRepository<InspectionPlanEntity>, ITransientDependency
{
    /// <summary>
    /// 获取设备的点检计划
    /// </summary>
    Task<List<InspectionPlanEntity>> GetByEquipmentIdAsync(long equipmentId);

    /// <summary>
    /// 获取需要执行的点检计划
    /// </summary>
    Task<List<InspectionPlanEntity>> GetPendingPlansAsync(DateTime beforeDate);

    /// <summary>
    /// 更新下次点检日期
    /// </summary>
    Task UpdateNextInspectDateAsync(long planId, DateTime nextDate);
}

/// <summary>
/// 点检记录仓储接口
/// </summary>
public interface IInspectionRecordRepository : ISqlSugarRepository<InspectionRecordEntity>, ITransientDependency
{
    /// <summary>
    /// 获取设备的点检记录
    /// </summary>
    Task<List<InspectionRecordEntity>> GetByEquipmentIdAsync(long equipmentId);

    /// <summary>
    /// 获取今日点检记录
    /// </summary>
    Task<List<InspectionRecordEntity>> GetTodayRecordsAsync(long? inspectorId);

    /// <summary>
    /// 获取设备的点检统计
    /// </summary>
    Task<(int totalCount, int normalCount, int abnormalCount)> GetStatisticsAsync(long? equipmentId, DateTime? fromDate, DateTime? toDate);
}

/// <summary>
/// 点检明细仓储接口
/// </summary>
public interface IInspectionItemRepository : ISqlSugarRepository<InspectionItemEntity>, ITransientDependency
{
    /// <summary>
    /// 根据计划ID获取所有明细
    /// </summary>
    Task<List<InspectionItemEntity>> GetByPlanIdAsync(long planId);

    /// <summary>
    /// 删除计划的所有明细
    /// </summary>
    Task DeleteByPlanIdAsync(long planId);
}
