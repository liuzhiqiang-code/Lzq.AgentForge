using Lzq.Extensions.ExternalHttpApi;
using Lzq.Extensions.Jwt.Models;
using Lzq.Rbac.Application.Contracts.Dtos;
using WebApiClientCore.Attributes;

namespace Lzq.Rbac.Application.Contracts.HttpApis;

public interface IAuthApi : IExternalHttpApi
{
    [HttpGet]
    Task<TokenViewDto> Login(UserLoginDto userLoginDto);

    [HttpGet]
    Task<UserInfoViewDto> GetUserInfo();
    Task CreateRole(RoleModel roleModel);
    Task DeleteRole(List<RoleModel> deleteRoleModels);
    Task UpdateRole(RoleUpdateModel roleUpdateModel);
}
