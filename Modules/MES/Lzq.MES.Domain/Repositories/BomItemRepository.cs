using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

public class BomItemRepository : SqlSugarRepository<BomItemEntity>, IBomItemRepository
{
    public BomItemRepository() { }
    public BomItemRepository(ISqlSugarClient client) : base(client) { }
}

