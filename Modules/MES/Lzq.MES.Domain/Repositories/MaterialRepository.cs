using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

public class MaterialRepository : SqlSugarRepository<MaterialEntity>, IMaterialRepository
{
    public MaterialRepository() { }
    public MaterialRepository(ISqlSugarClient client) : base(client) { }
}

