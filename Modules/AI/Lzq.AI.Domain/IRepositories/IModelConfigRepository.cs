using Lzq.AI.Domain.Entities;
using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.AI.Domain.IRepositories;

public interface IModelConfigRepository : ISqlSugarRepository<ModelConfigEntity>, ITransientDependency
{

}