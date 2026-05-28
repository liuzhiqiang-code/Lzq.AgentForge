using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

/// <summary>
/// 报修单仓储实现
/// </summary>
public class RepairOrderRepository : SqlSugarRepository<RepairOrderEntity>, IRepairOrderRepository
{
    public RepairOrderRepository() { }
    public RepairOrderRepository(ISqlSugarClient client) : base(client) { }

    public async Task<List<RepairOrderEntity>> GetByEquipmentIdAsync(long equipmentId)
    {
        return await AsQueryable()
            .Where(x => x.EquipmentId == equipmentId)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync();
    }

    public async Task<List<RepairOrderEntity>> GetByStatusAsync(Enums.RepairStatusEnum status)
    {
        return await AsQueryable()
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync();
    }

    public async Task<List<RepairOrderEntity>> GetPendingOrdersAsync()
    {
        return await AsQueryable()
            .Where(x => x.Status == Domain.Enums.RepairStatusEnum.Pending)
            .OrderBy(x => x.CreationTime)
            .ToListAsync();
    }

    public async Task<(int pendingCount, int inProgressCount, int completedCount)> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate)
    {
        var query = AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.CreationTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.CreationTime <= toDate.Value);
        }

        var pendingCount = await query.Clone().Where(x => x.Status == Domain.Enums.RepairStatusEnum.Pending).CountAsync();
        var inProgressCount = await query.Clone().Where(x => x.Status == Domain.Enums.RepairStatusEnum.InProgress || x.Status == Domain.Enums.RepairStatusEnum.Assigned).CountAsync();
        var completedCount = await query.Clone().Where(x => x.Status == Domain.Enums.RepairStatusEnum.Completed || x.Status == Domain.Enums.RepairStatusEnum.Accepted).CountAsync();

        return (pendingCount, inProgressCount, completedCount);
    }
}

/// <summary>
/// 保养计划仓储实现
/// </summary>
public class MaintenancePlanRepository : SqlSugarRepository<MaintenancePlanEntity>, IMaintenancePlanRepository
{
    public MaintenancePlanRepository() { }
    public MaintenancePlanRepository(ISqlSugarClient client) : base(client) { }

    public async Task<List<MaintenancePlanEntity>> GetByEquipmentIdAsync(long equipmentId)
    {
        return await AsQueryable()
            .Where(x => x.EquipmentId == equipmentId)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync();
    }

    public async Task<List<MaintenancePlanEntity>> GetPendingPlansAsync(DateTime beforeDate)
    {
        return await AsQueryable()
            .Where(x => x.IsEnabled == true)
            .Where(x => x.NextMaintenanceDate == null || x.NextMaintenanceDate <= beforeDate)
            .OrderBy(x => x.NextMaintenanceDate)
            .ToListAsync();
    }

    public async Task<List<MaintenancePlanEntity>> GetTodayPlansAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await AsQueryable()
            .Where(x => x.IsEnabled == true)
            .Where(x => x.NextMaintenanceDate >= today && x.NextMaintenanceDate < tomorrow)
            .OrderBy(x => x.NextMaintenanceDate)
            .ToListAsync();
    }
}

/// <summary>
/// 保养记录仓储实现
/// </summary>
public class MaintenanceRecordRepository : SqlSugarRepository<MaintenanceRecordEntity>, IMaintenanceRecordRepository
{
    public MaintenanceRecordRepository() { }
    public MaintenanceRecordRepository(ISqlSugarClient client) : base(client) { }

    public async Task<List<MaintenanceRecordEntity>> GetByEquipmentIdAsync(long equipmentId)
    {
        return await AsQueryable()
            .Where(x => x.EquipmentId == equipmentId)
            .OrderByDescending(x => x.MaintenanceDate)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(long? equipmentId, DateTime? fromDate, DateTime? toDate)
    {
        var query = AsQueryable();

        if (equipmentId.HasValue)
        {
            query = query.Where(x => x.EquipmentId == equipmentId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.MaintenanceDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.MaintenanceDate <= toDate.Value);
        }

        return await query.CountAsync();
    }
}
