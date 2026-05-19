using Lzq.BaseData.Domain.Entities;
using Lzq.BaseData.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.BaseData.Domain.Repositories;

public class LineRepository : SqlSugarRepository<LineEntity>, ILineRepository
{
    public LineRepository() { }

    public LineRepository(ISqlSugarClient client) : base(client) { }
}
