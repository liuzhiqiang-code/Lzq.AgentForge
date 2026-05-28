using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

public class UnitOfMeasureRepository : SqlSugarRepository<UnitOfMeasureEntity>, IUnitOfMeasureRepository
{
    public UnitOfMeasureRepository() { }
    public UnitOfMeasureRepository(ISqlSugarClient client) : base(client) { }
}

