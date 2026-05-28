using Lzq.Extensions.SqlSugar.Repository;
using Lzq.MES.Domain.Entities.WorkOrder;
using Lzq.MES.Domain.IRepositories.WorkOrder;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories.WorkOrder;

/// <summary>
/// 工单仓储实现
/// </summary>
public class WorkOrderRepository : SqlSugarRepository<WorkOrderEntity>, IWorkOrderRepository
{
    public WorkOrderRepository() { }
    public WorkOrderRepository(ISqlSugarClient client) : base(client) { }

    public async Task<WorkOrderEntity?> GetByCodeAsync(string code)
    {
        return await AsQueryable()
            .Where(x => x.Code == code)
            .FirstAsync();
    }

    public async Task<List<WorkOrderEntity>> GetByStatusAsync(Lzq.MES.Domain.Enums.WorkOrderStatusEnum status)
    {
        return await AsQueryable()
            .Where(x => x.Status == status)
            .ToListAsync();
    }

    public async Task<List<WorkOrderEntity>> GetByLineIdAsync(long lineId)
    {
        return await AsQueryable()
            .Where(x => x.LineId == lineId)
            .ToListAsync();
    }

    public async Task<bool> ExistsByCodeAsync(string code, long? excludeId = null)
    {
        var query = AsQueryable()
            .Where(x => x.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
