using Lzq.Rbac.Application.Contracts.Commands;
using Lzq.Rbac.Application.Contracts.Queries;


/*
 * @author : liuzhiqiang
 * @date : 2026-1-1
 * @desc : user
 */
namespace Lzq.Rbac.Application.Contracts.IServices;

public interface IUserService
{
    Task<ApiResult> PageAsync(UserPageQuery query);
    Task<ApiResult> ListAsync();
    Task<ApiResult> CreateAsync(UserCreateCommand command);
    Task<ApiResult> UpdateAsync(UserUpdateCommand command);
    Task<ApiResult> DeleteAsync(long id);
    Task<ApiResult> BatchDeleteAsync(List<long> ids);
}