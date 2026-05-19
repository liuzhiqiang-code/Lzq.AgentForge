using Lzq.Equipment.Domain.Entities;
using Lzq.Equipment.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.Equipment.Domain.Repositories;

/// <summary>
/// 点检计划仓储实现
/// </summary>
public class InspectionPlanRepository : SqlSugarRepository<InspectionPlanEntity>, IInspectionPlanRepository
{
    public InspectionPlanRepository() { }
    public InspectionPlanRepository(ISqlSugarClient client) : base(client) { }

    public async Task<List<InspectionPlanEntity>> GetByEquipmentIdAsync(long equipmentId)
    {
        return await AsQueryable()
            .Where(x => x.EquipmentId == equipmentId)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync();
    }

    public async Task<List<InspectionPlanEntity>> GetPendingPlansAsync(DateTime beforeDate)
    {
        return await AsQueryable()
            .Where(x => x.IsEnabled == true)
            .Where(x => x.NextInspectDate == null || x.NextInspectDate <= beforeDate)
            .OrderBy(x => x.NextInspectDate)
            .ToListAsync();
    }

    public async Task UpdateNextInspectDateAsync(long planId, DateTime nextDate)
    {
        await AsUpdateable()
            .SetColumns(x => x.NextInspectDate == nextDate)
            .Where(x => x.Id == planId)
            .ExecuteCommandAsync();
    }
}

/// <summary>
/// 点检记录仓储实现
/// </summary>
public class InspectionRecordRepository : SqlSugarRepository<InspectionRecordEntity>, IInspectionRecordRepository
{
    public InspectionRecordRepository() { }
    public InspectionRecordRepository(ISqlSugarClient client) : base(client) { }

    public async Task<List<InspectionRecordEntity>> GetByEquipmentIdAsync(long equipmentId)
    {
        return await AsQueryable()
            .Where(x => x.EquipmentId == equipmentId)
            .OrderByDescending(x => x.InspectDate)
            .ToListAsync();
    }

    public async Task<List<InspectionRecordEntity>> GetTodayRecordsAsync(long? inspectorId)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var query = AsQueryable()
            .Where(x => x.InspectDate >= today && x.InspectDate < tomorrow);

        if (inspectorId.HasValue)
        {
            query = query.Where(x => x.InspectorId == inspectorId.Value);
        }

        return await query.OrderByDescending(x => x.InspectDate).ToListAsync();
    }

    public async Task<(int totalCount, int normalCount, int abnormalCount)> GetStatisticsAsync(long? equipmentId, DateTime? fromDate, DateTime? toDate)
    {
        var query = AsQueryable();

        if (equipmentId.HasValue)
        {
            query = query.Where(x => x.EquipmentId == equipmentId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.InspectDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.InspectDate <= toDate.Value);
        }

        var totalCount = await query.CountAsync();
        var normalCount = await query.Where(x => x.Result == Domain.Enums.InspectionResultEnum.Normal).CountAsync();
        var abnormalCount = await query.Where(x => x.Result == Domain.Enums.InspectionResultEnum.Abnormal).CountAsync();

        return (totalCount, normalCount, abnormalCount);
    }
}

/// <summary>
/// 点检明细仓储实现
/// </summary>
public class InspectionItemRepository : SqlSugarRepository<InspectionItemEntity>, IInspectionItemRepository
{
    public InspectionItemRepository() { }
    public InspectionItemRepository(ISqlSugarClient client) : base(client) { }

    public async Task<List<InspectionItemEntity>> GetByPlanIdAsync(long planId)
    {
        return await AsQueryable()
            .Where(x => x.PlanId == planId)
            .OrderBy(x => x.Sort)
            .ToListAsync();
    }

    public async Task DeleteByPlanIdAsync(long planId)
    {
        await AsDeleteable()
            .Where(x => x.PlanId == planId)
            .ExecuteCommandAsync();
    }
}
