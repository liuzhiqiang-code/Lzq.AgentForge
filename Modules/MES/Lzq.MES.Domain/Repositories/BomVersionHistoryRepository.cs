using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

public class BomVersionHistoryRepository : SqlSugarRepository<BomVersionHistoryEntity>, IBomVersionHistoryRepository
{
    public BomVersionHistoryRepository() { }
    public BomVersionHistoryRepository(ISqlSugarClient client) : base(client) { }
}

