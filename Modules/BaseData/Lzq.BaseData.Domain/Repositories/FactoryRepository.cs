using Lzq.BaseData.Domain.Entities;
using Lzq.BaseData.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.BaseData.Domain.Repositories;

public class FactoryRepository : SqlSugarRepository<FactoryEntity>, IFactoryRepository
{
    public FactoryRepository() { }

    public FactoryRepository(ISqlSugarClient client) : base(client) { }
}
