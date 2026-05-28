using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

public class FactoryRepository : SqlSugarRepository<FactoryEntity>, IFactoryRepository
{
    public FactoryRepository() { }

    public FactoryRepository(ISqlSugarClient client) : base(client) { }
}
