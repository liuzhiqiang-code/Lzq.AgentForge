using Lzq.Extensions.Jwt.Models;
using Lzq.Rbac.Application.Contracts.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.Rbac.Application.Contracts.IServices;

public interface IAccountService : ITransientDependency
{
    Task<ApiResult<TokenViewDto>> LoginAsync(LoginCommand command);
    Task<ApiResult> RegisterAsync(RegisterCommand command);
    Task<ApiResult> LogoutAsync();
    Task<ApiResult> UserInfoAsync();
}