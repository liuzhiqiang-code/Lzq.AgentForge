using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

public class ProcessRepository : SqlSugarRepository<ProcessEntity>, IProcessRepository
{
    public ProcessRepository() { }

    public ProcessRepository(ISqlSugarClient client) : base(client) { }
}
