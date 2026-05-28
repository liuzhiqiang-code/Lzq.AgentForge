using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

public class MaterialTypeRepository : SqlSugarRepository<MaterialTypeEntity>, IMaterialTypeRepository
{
    public MaterialTypeRepository() { }
    public MaterialTypeRepository(ISqlSugarClient client) : base(client) { }
}

