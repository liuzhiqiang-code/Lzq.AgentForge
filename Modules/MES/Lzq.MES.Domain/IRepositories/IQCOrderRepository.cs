using Lzq.MES.Domain.Enums;
using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;
using Lzq.MES.Domain.Entities;

namespace Lzq.MES.Domain.IRepositories;

/// <summary>
/// 质检单仓储接口
/// </summary>
public interface IQCOrderRepository : ISqlSugarRepository<QCOrderEntity>, ITransientDependency
{
    /// <summary>
    /// 根据编号查询
    /// </summary>
    Task<QCOrderEntity?> GetByCodeAsync(string code);

    /// <summary>
    /// 根据类型和状态查询质检单
    /// </summary>
    Task<List<QCOrderEntity>> GetByTypeAndStatusAsync(QCTypeEnum qcType, QCOrderStatusEnum status);

    /// <summary>
    /// 根据关联单据查询质检单
    /// </summary>
    Task<List<QCOrderEntity>> GetByRefIdAsync(long refId);

    /// <summary>
    /// 检查质检单编号是否存在
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, long? excludeId = null);
}

/// <summary>
/// 质检明细仓储接口
/// </summary>
public interface IQCOrderItemRepository : ISqlSugarRepository<QCOrderItemEntity>, ITransientDependency
{
    /// <summary>
    /// 根据质检单ID查询所有明细
    /// </summary>
    Task<List<QCOrderItemEntity>> GetByQCOrderIdAsync(long qcOrderId);

    /// <summary>
    /// 删除指定质检单的所有明细
    /// </summary>
    Task DeleteByQCOrderIdAsync(long qcOrderId);
}
