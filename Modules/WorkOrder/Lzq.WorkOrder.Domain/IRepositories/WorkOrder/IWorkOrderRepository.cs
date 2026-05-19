using Lzq.WorkOrder.Domain.Entities.WorkOrder;
using Lzq.WorkOrder.Domain.Enums;
using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.WorkOrder.Domain.IRepositories.WorkOrder;

/// <summary>
/// 工单仓储接口
/// </summary>
public interface IWorkOrderRepository : ISqlSugarRepository<WorkOrderEntity>, ITransientDependency
{
    /// <summary>
    /// 根据工单编号查询
    /// </summary>
    Task<WorkOrderEntity?> GetByCodeAsync(string code);

    /// <summary>
    /// 根据状态查询工单列表
    /// </summary>
    Task<List<WorkOrderEntity>> GetByStatusAsync(WorkOrderStatusEnum status);

    /// <summary>
    /// 根据产线ID查询工单列表
    /// </summary>
    Task<List<WorkOrderEntity>> GetByLineIdAsync(long lineId);

    /// <summary>
    /// 检查工单编号是否存在
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, long? excludeId = null);
}
