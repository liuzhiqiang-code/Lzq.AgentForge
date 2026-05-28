using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

public class LineRepository : SqlSugarRepository<LineEntity>, ILineRepository
{
    public LineRepository() { }

    public LineRepository(ISqlSugarClient client) : base(client) { }
}
