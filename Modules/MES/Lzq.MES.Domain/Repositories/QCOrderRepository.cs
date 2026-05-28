using Lzq.Extensions.SqlSugar.Repository;
using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

/// <summary>
/// 质检单仓储实现
/// </summary>
public class QCOrderRepository : SqlSugarRepository<QCOrderEntity>, IQCOrderRepository
{
    public QCOrderRepository()
    {
    }
    public QCOrderRepository(ISqlSugarClient client) : base(client) { }

    public async Task<QCOrderEntity?> GetByCodeAsync(string code)
    {
        return await AsQueryable()
            .Where(x => x.Code == code)
            .FirstAsync();
    }

    public async Task<List<QCOrderEntity>> GetByTypeAndStatusAsync(Enums.QCTypeEnum qcType, Enums.QCOrderStatusEnum status)
    {
        return await AsQueryable()
            .Where(x => x.QCType == qcType && x.Status == status)
            .ToListAsync();
    }

    public async Task<List<QCOrderEntity>> GetByRefIdAsync(long refId)
    {
        return await AsQueryable()
            .Where(x => x.RefId == refId)
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

/// <summary>
/// 质检明细仓储实现
/// </summary>
public class QCOrderItemRepository : SqlSugarRepository<QCOrderItemEntity>, IQCOrderItemRepository
{
    public QCOrderItemRepository()
    {
    }
    public QCOrderItemRepository(ISqlSugarClient client) : base(client) { }

    public async Task<List<QCOrderItemEntity>> GetByQCOrderIdAsync(long qcOrderId)
    {
        return await AsQueryable()
            .Where(x => x.QCOrderId == qcOrderId)
            .ToListAsync();
    }

    public async Task DeleteByQCOrderIdAsync(long qcOrderId)
    {
        await AsDeleteable()
            .Where(x => x.QCOrderId == qcOrderId)
            .ExecuteCommandAsync();
    }
}
