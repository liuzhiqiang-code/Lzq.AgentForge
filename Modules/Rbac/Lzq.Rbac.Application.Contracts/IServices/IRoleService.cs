using Lzq.Rbac.Application.Contracts.Commands;
using Lzq.Rbac.Application.Contracts.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.Rbac.Application.Contracts.IServices;

public interface IRoleService : ITransientDependency
{
    Task<ApiResult> PageAsync(RolePageQuery query);
    Task<ApiResult> ListAsync();
    Task<ApiResult> CreateAsync(RoleCreateCommand command);
    Task<ApiResult> UpdateAsync(RoleUpdateCommand command);
    Task<ApiResult> DeleteAsync(long id);
    Task<ApiResult> BatchDeleteAsync(List<long> ids);
}
