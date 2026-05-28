using Lzq.MES.Domain.Entities;
using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.MES.Domain.IRepositories;

public interface IProcessRepository : ISqlSugarRepository<ProcessEntity>, ITransientDependency
{
}
