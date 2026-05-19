using Lzq.WorkOrder.Domain.Entities.WorkOrder;
using Lzq.WorkOrder.Domain.IRepositories.WorkOrder;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.WorkOrder.Domain.Repositories.WorkOrder;

/// <summary>
/// 报工记录仓储实现
/// </summary>
public class WorkReportRepository : SqlSugarRepository<WorkReportEntity>, IWorkReportRepository
{
    public WorkReportRepository() { }
    public WorkReportRepository(ISqlSugarClient client) : base(client) { }

    /// <summary>
    /// 根据工单ID查询报工记录列表
    /// </summary>
    public async Task<List<WorkReportEntity>> GetByWorkOrderIdAsync(long workOrderId)
    {
        return await AsQueryable()
            .Where(x => x.WorkOrderId == workOrderId)
            .OrderByDescending(x => x.ReportTime)
            .ToListAsync();
    }

    /// <summary>
    /// 统计工单的总报工数量
    /// </summary>
    public async Task<(int qualifiedQty, int defectQty)> GetTotalQtyByWorkOrderIdAsync(long workOrderId)
    {
        var reports = await AsQueryable()
            .Where(x => x.WorkOrderId == workOrderId)
            .Select(x => new { x.QualifiedQty, x.DefectQty })
            .ToListAsync();

        var qualifiedQty = reports.Sum(x => x.QualifiedQty);
        var defectQty = reports.Sum(x => x.DefectQty);

        return (qualifiedQty, defectQty);
    }
}
