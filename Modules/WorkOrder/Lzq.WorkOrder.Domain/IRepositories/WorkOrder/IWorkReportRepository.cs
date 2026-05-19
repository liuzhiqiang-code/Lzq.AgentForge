using Lzq.WorkOrder.Domain.Entities.WorkOrder;
using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.WorkOrder.Domain.IRepositories.WorkOrder;

/// <summary>
/// 报工记录仓储接口
/// </summary>
public interface IWorkReportRepository : ISqlSugarRepository<WorkReportEntity>, ITransientDependency
{
    /// <summary>
    /// 根据工单ID查询报工记录列表
    /// </summary>
    Task<List<WorkReportEntity>> GetByWorkOrderIdAsync(long workOrderId);

    /// <summary>
    /// 统计工单的总报工数量
    /// </summary>
    Task<(int qualifiedQty, int defectQty)> GetTotalQtyByWorkOrderIdAsync(long workOrderId);
}
