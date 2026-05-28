using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

public class EcnItemRepository : SqlSugarRepository<EcnItemEntity>, IEcnItemRepository
{
    public EcnItemRepository() { }
    public EcnItemRepository(ISqlSugarClient client) : base(client) { }
}

