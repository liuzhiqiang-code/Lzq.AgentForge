using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

public class EcnRepository : SqlSugarRepository<EcnEntity>, IEcnRepository
{
    public EcnRepository() { }
    public EcnRepository(ISqlSugarClient client) : base(client) { }
}

