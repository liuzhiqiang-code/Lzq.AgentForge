using Lzq.Core.Models;
using Lzq.Rbac.Application.Contracts.Commands;
using Lzq.Rbac.Application.Contracts.Dtos;
using Lzq.Rbac.Application.Contracts.HttpApis;
using Lzq.Rbac.Application.Contracts.IServices;
using Lzq.Rbac.Application.Contracts.Queries;
using Lzq.Rbac.Domain.Entities;
using Lzq.Rbac.Domain.IRepositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SqlSugar;

namespace Lzq.Rbac.Application.Services;

public class RoleService : ServiceBase, IRoleService
{
    public RoleService() : base("/api/v1/rbac/role") { }
    private IRoleRepository RoleRepository => GetRequiredService<IRoleRepository>();
    private IRoleAuthRepository RoleAuthRepository => GetRequiredService<IRoleAuthRepository>();
    private IAuthApi AuthApi => GetRequiredService<IAuthApi>();


    [OpenApiTag("rbac/role"), OpenApiOperation("获取角色分页列表", "")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult> PageAsync([FromBody] RolePageQuery query)
    {
        RefAsync<int> total = 0;
        var pageList = await RoleRepository.AsQueryable().ToPageListAsync(query.Page, query.PageSize, total);
        var roleIds = pageList.Select(r => r.Id).ToList();
        var rolePermissionsDict = await GetRolePermissionsDictionaryAsync(roleIds);
        var result = pageList.Select(role =>
        {
            var roleDto = role.Map<RoleViewDto>();
            if (rolePermissionsDict.TryGetValue(role.Id, out var permissions))
                roleDto.Permissions = permissions;
            else
                roleDto.Permissions = [];
            return roleDto;
        }).ToList();

        return ApiResult.Success(new PagedResponse<RoleViewDto>(result, total));
    }

    private async Task<Dictionary<long, List<long>>> GetRolePermissionsDictionaryAsync(List<long> roleIds)
    {
        var rolePermissions = await RoleAuthRepository
            .GetListAsync(a => roleIds.Contains(a.RoleId));
        return rolePermissions
            .GroupBy(ra => ra.RoleId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(ra => ra.MenuId).ToList()
            );
    }

    [OpenApiTag("rbac/role"), OpenApiOperation("获取角色列表", "")]
    [RoutePattern(pattern: "list", true)]
    public async Task<ApiResult> ListAsync()
    {
        var list = (await RoleRepository.GetListAsync()).ToList();
        return ApiResult.Success(list.Map<List<RoleViewDto>>());
    }

    [OpenApiTag("rbac/role"), OpenApiOperation("增加角色", "")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult> CreateAsync([FromBody] RoleCreateCommand command)
    {
        var entity = command.Map<RoleEntity>();
        await RoleRepository.InsertAsync(entity);
        var rolePermissions = command.Permissions.Select(permissionId => new RoleAuthEntity
        {
            RoleId = entity.Id,
            MenuId = permissionId
        }).ToList();

        if (rolePermissions.Count > 0)
            await RoleAuthRepository.InsertRangeAsync(rolePermissions);

        if (false)//微服务
        {
            await AuthApi.CreateRole(new RoleModel
            {
                Name = entity.Name
            });
        }
        return ApiResult.Success();
    }

    [OpenApiTag("rbac/role"), OpenApiOperation("更新角色", "")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult> UpdateAsync([FromBody] RoleUpdateCommand command)
    {
        var entity = await RoleRepository.GetByIdAsync(command.Id);
        if (entity == null)
            throw new MasaValidatorException("Role not found");
        command.Map(entity);
        await RoleRepository.UpdateAsync(entity);
        await UpdateRolePermissionsAsync(command.Id, command.Permissions);

        var roleUpdateModel = new RoleUpdateModel
        {
            Name = entity.Name,
            NewRoleName = command.Name
        };

        if (false)//微服务
            await AuthApi.UpdateRole(roleUpdateModel);
        return ApiResult.Success();
    }

    /// <summary>
    /// 更新角色权限
    /// </summary>
    private async Task UpdateRolePermissionsAsync(long roleId, List<long> newPermissionIds)
    {
        if (newPermissionIds == null)
            return;

        // 1. 获取现有的权限
        var existingPermissions = await RoleAuthRepository
            .GetListAsync(a => a.RoleId == roleId);

        // 2. 找出需要删除的权限（存在但现在不需要了）
        var permissionsToDelete = existingPermissions
            .Where(ep => !newPermissionIds.Contains(ep.MenuId))
            .ToList();

        if (permissionsToDelete.Any())
        {
            await RoleAuthRepository.DeleteAsync(a =>
                a.RoleId == roleId &&
                permissionsToDelete.Select(p => p.MenuId).Contains(a.MenuId));
        }

        // 3. 找出需要新增的权限（现在需要但之前没有的）
        var existingPermissionIds = existingPermissions.Select(ep => ep.MenuId).ToList();
        var permissionsToAdd = newPermissionIds
            .Where(pid => !existingPermissionIds.Contains(pid))
            .Select(permissionId => new RoleAuthEntity
            {
                RoleId = roleId,
                MenuId = permissionId
            })
            .ToList();

        if (permissionsToAdd.Any())
        {
            await RoleAuthRepository.InsertRangeAsync(permissionsToAdd);
        }
    }

    [OpenApiTag("rbac/role"), OpenApiOperation("删除角色", "")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult> DeleteAsync(long id)
    {
        var list = await RoleRepository.GetListAsync(a => id.Equals(a.Id));
        await DeleteRolePermissionsAsync(new List<long> { id });
        await RoleRepository.DeleteAsync(a => id.Equals(a.Id));

        var deleteRoleModels = list.Select(a => new RoleModel
        {
            Name = a.Name
        }).ToList();

        if (false)//微服务
            await AuthApi.DeleteRole(deleteRoleModels);
        return ApiResult.Success();
    }

    [OpenApiTag("rbac/role"), OpenApiOperation("批量删除角色", "")]
    [RoutePattern(pattern: "batchDelete", true, HttpMethod = "Delete")]
    public async Task<ApiResult> BatchDeleteAsync([FromBody] List<long> ids)
    {
        var list = await RoleRepository.GetListAsync(a => ids.Contains(a.Id));
        await DeleteRolePermissionsAsync(ids);
        await RoleRepository.DeleteAsync(a => ids.Contains(a.Id));
        var deleteRoleModels = list.Select(a => new RoleModel
        {
            Name = a.Name
        }).ToList();

        if (false)//微服务
            await AuthApi.DeleteRole(deleteRoleModels);
        return ApiResult.Success();
    }
    private async Task DeleteRolePermissionsAsync(List<long> roleIds)
    {
        if (roleIds == null || !roleIds.Any())
            return;

        await RoleAuthRepository.DeleteAsync(a => roleIds.Contains(a.RoleId));
    }
}
