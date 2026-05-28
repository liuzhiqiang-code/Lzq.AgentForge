using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

public class WorkshopRepository : SqlSugarRepository<WorkshopEntity>, IWorkshopRepository
{
    public WorkshopRepository() { }

    public WorkshopRepository(ISqlSugarClient client) : base(client) { }
}
