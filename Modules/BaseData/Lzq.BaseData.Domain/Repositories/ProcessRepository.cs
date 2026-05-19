using Lzq.BaseData.Domain.Entities;
using Lzq.BaseData.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.BaseData.Domain.Repositories;

public class ProcessRepository : SqlSugarRepository<ProcessEntity>, IProcessRepository
{
    public ProcessRepository() { }

    public ProcessRepository(ISqlSugarClient client) : base(client) { }
}
