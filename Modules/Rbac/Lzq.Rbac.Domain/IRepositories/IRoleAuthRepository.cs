using Lzq.Extensions.SqlSugar.Repository;
using Lzq.Rbac.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.Rbac.Domain.IRepositories;

public interface IRoleAuthRepository : ISqlSugarRepository<RoleAuthEntity>, ITransientDependency
{

}