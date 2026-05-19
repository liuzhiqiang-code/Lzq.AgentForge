using Lzq.BaseData.Domain.Entities;
using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.BaseData.Domain.IRepositories;

public interface IProcessRepository : ISqlSugarRepository<ProcessEntity>, ITransientDependency
{
}
