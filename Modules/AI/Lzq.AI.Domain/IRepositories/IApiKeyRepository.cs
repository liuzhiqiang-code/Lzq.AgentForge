using Lzq.AI.Domain.Entities;
using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.AI.Domain.IRepositories;

public interface IApiKeyRepository : ISqlSugarRepository<ApiKeyEntity>, ITransientDependency
{

}