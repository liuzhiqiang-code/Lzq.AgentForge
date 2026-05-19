using Lzq.BaseData.Domain.Entities;
using Lzq.BaseData.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.BaseData.Domain.Repositories;

public class WorkshopRepository : SqlSugarRepository<WorkshopEntity>, IWorkshopRepository
{
    public WorkshopRepository() { }

    public WorkshopRepository(ISqlSugarClient client) : base(client) { }
}
