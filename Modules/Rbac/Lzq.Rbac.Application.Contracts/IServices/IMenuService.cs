using Lzq.Rbac.Application.Contracts.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.Rbac.Application.Contracts.IServices;

public interface IMenuService : ITransientDependency
{
    Task<ApiResult> NameExistsAsync(long? id, string? path);
    Task<ApiResult> PathExistsAsync(long? id, string? path);
    Task<ApiResult> ListAsync();
    Task<ApiResult> AllAsync();
    Task<ApiResult> CreateAsync(MenuCreateCommand command);
    Task<ApiResult> UpdateAsync(MenuUpdateCommand command);
    Task<ApiResult> DeleteAsync(long id);
    Task<ApiResult> BatchDeleteAsync(List<long> ids);
}
