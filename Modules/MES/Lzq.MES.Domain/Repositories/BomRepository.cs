using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

public class BomRepository : SqlSugarRepository<BomEntity>, IBomRepository
{
    public BomRepository() { }
    public BomRepository(ISqlSugarClient client) : base(client) { }
}

