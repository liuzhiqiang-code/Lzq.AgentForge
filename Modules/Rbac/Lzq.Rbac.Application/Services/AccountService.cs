using Lzq.Core.Interfaces;
using Lzq.Core.Utils;
using Lzq.Extensions.Jwt;
using Lzq.Extensions.Jwt.Models;
using Lzq.Extensions.Jwt.Services;
using Lzq.Rbac.Application.Contracts.Commands;
using Lzq.Rbac.Application.Contracts.Dtos;
using Lzq.Rbac.Application.Contracts.HttpApis;
using Lzq.Rbac.Application.Contracts.IServices;
using Lzq.Rbac.Domain.Entities;
using Lzq.Rbac.Domain.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

/*
 * @author : liuzhiqiang
 * @date : 2026-1-1
 * @desc : user
 */
namespace Lzq.Rbac.Application.Services;

public class AccountService : ServiceBase, IAccountService
{
    public AccountService() : base("/api/v1/rbac/account") { }
    private IUserRepository UserRepository => GetRequiredService<IUserRepository>();
    private ITokenGenerator TokenGenerator => GetRequiredService<ITokenGenerator>();
    private ICurrentUser CurrentUser => GetRequiredService<ICurrentUser>();
    private IAuthApi AuthApi => GetRequiredService<IAuthApi>();


    [OpenApiTag("rbac/account"), OpenApiOperation("登录", ""), AllowAnonymous]
    [RoutePattern(pattern: "login", true)]
    public async Task<ApiResult<TokenResult>> LoginAsync([FromBody] LoginCommand command)
    {
        var user = await UserRepository.GetFirstAsync(a => a.UserName.Equals(command.UserName));
        if (user == null)
            throw new MasaException("用户不存在");
        if (!user.Password.Equals(command.Password))
            throw new MasaException("密码错误");

        TokenResult? result;
        if (false)//微服务
            result = await AuthApi.Login(new UserLoginDto(command.UserName!, command.Password!));
        else
        {
            ICurrentUser currentUser = new CurrentUser()
                .SetUserId(user.Id.ToString())
                .SetUserName(user.UserName)
                .SetEmail(user.Email)
                .SetSex(user.Sex.ToString() ?? "")
                .SetTenantId("")//暂时不管租户
                .SetRoles(user.Roles);

            result = TokenGenerator.Generate(currentUser, TimeSpan.FromHours(2));
        }

        if (result == null)
            throw new MasaException("登录失败");
        return ApiResult.Success(result);
    }

    [OpenApiTag("rbac/account"), OpenApiOperation("注册", ""), AllowAnonymous]
    [RoutePattern(pattern: "register", true)]
    public async Task<ApiResult> RegisterAsync([FromBody] RegisterCommand command)
    {
        var user = await UserRepository.GetFirstAsync(a => a.UserName.Equals(command.UserName));
        if (user != null)
            throw new UserFriendlyException("账户已被注册");
        var entity = new UserEntity
        {
            UserName = command.UserName!,
            Password =  command.Password!,
            Roles = new List<string> { "2" }
        };
        await UserRepository.InsertAsync(entity);
        return ApiResult.Success();
    }

    [OpenApiTag("rbac/account"), OpenApiOperation("登出", ""), AllowAnonymous]
    [RoutePattern(pattern: "logout", true)]
    public async Task<ApiResult> LogoutAsync()
    {
        return ApiResult.Success();
    }

    [OpenApiTag("rbac/account"), OpenApiOperation("获取用户信息", "")]
    [RoutePattern(pattern: "userInfo", true, HttpMethod = "Get")]
    public async Task<ApiResult> UserInfoAsync()
    {
        UserInfoViewDto? result;
        if (false)//微服务
            result = await AuthApi.GetUserInfo();
        else
        {
            result = new UserInfoViewDto
            {
                Id = CurrentUser.UserId.ToInt64(),
                UserName = CurrentUser.UserName,
                Email = CurrentUser.Email,
            };
        }
        if (result == null)
            throw new MasaException("获取用户信息失败");
        // 这里可以根据token得到的用户名信息查更多用户信息
        return ApiResult.Success(result);
    }
}
