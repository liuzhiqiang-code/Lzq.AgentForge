using Lzq.Rbac.Application.Contracts.Commands;
using Lzq.Rbac.Application.Contracts.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.Rbac.Application.Contracts.IServices;

public interface IDeptService : ITransientDependency
{
    Task<ApiResult> PageAsync(DeptPageQuery query);
    Task<ApiResult> ListAsync(DeptListQuery query);
    Task<ApiResult> CreateAsync(DeptCreateCommand command);
    Task<ApiResult> UpdateAsync(DeptUpdateCommand command);
    Task<ApiResult> DeleteAsync(long id);
    Task<ApiResult> BatchDeleteAsync(List<long> ids);
}
