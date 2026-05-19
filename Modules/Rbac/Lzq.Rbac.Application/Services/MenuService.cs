using Lzq.Core.Interfaces;
using Lzq.Rbac.Application.Contracts.Commands;
using Lzq.Rbac.Application.Contracts.Dtos;
using Lzq.Rbac.Application.Contracts.IServices;
using Lzq.Rbac.Application.Contracts.Queries;
using Lzq.Rbac.Domain.Entities;
using Lzq.Rbac.Domain.IRepositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace Lzq.Rbac.Application.Services;

public class MenuService : ServiceBase, IMenuService
{
    public MenuService() : base("/api/v1/rbac/menu") { }
    private ICurrentUser CurrentUser => GetRequiredService<ICurrentUser>();
    private IMenuRepository MenuRepository => GetRequiredService<IMenuRepository>();
    private IRoleAuthRepository RoleAuthRepository => GetRequiredService<IRoleAuthRepository>();

    [OpenApiTag("rbac/menu"), OpenApiOperation("菜单名称是否存在", "")]
    [RoutePattern(pattern: "name-exists", true, HttpMethod = "GET")]
    public async Task<ApiResult> NameExistsAsync([FromQuery] long? id, string? path)
    {
        var query = new MenuNameExistsQuery(id, path);
        if (query.Name.IsNullOrWhiteSpace())
            return ApiResult.Success(false);

        var count = await MenuRepository.AsQueryable()
            .WhereIF(query.Id.HasValue, a => !a.Id.Equals(query.Id))
            .WhereIF(!query.Name.IsNullOrWhiteSpace(), a => a.Name.Equals(query.Name))
            .CountAsync();
        return ApiResult.Success(count > 0);
    }

    [OpenApiTag("rbac/menu"), OpenApiOperation("菜单路由是否存在", "")]
    [RoutePattern(pattern: "path-exists", true, HttpMethod = "GET")]
    public async Task<ApiResult> PathExistsAsync(long? id, string? path)
    {
        var query = new MenuPathExistsQuery(id, path);
        if (query.Path.IsNullOrWhiteSpace())
            return ApiResult.Success(false);

        var count = await MenuRepository.AsQueryable()
            .WhereIF(query.Id.HasValue, a => !a.Id.Equals(query.Id))
            .WhereIF(!query.Path.IsNullOrWhiteSpace(), a => a.Path!.Equals(query.Path))
            .CountAsync();
        return ApiResult.Success(count > 0);
    }

    [OpenApiTag("rbac/menu"), OpenApiOperation("获取菜单列表", "")]
    [RoutePattern(pattern: "list", true, HttpMethod = "GET")]
    public async Task<ApiResult> ListAsync()
    {
        // 获取所有菜单数据
        var allMenus = (await MenuRepository.GetListAsync())
            .ToList()
            .Map<List<MenuViewDto>>();

        // 构建树形结构
        return ApiResult.Success(BuildMenuTree(allMenus, null));
    }

    // 递归构建部门树
    private List<MenuViewDto> BuildMenuTree(List<MenuViewDto> allMenus, long? parentId)
    {
        return allMenus
            .Where(d => d.Pid == parentId)
            .Select(d => new MenuViewDto
            {
                Id = d.Id,
                Pid = d.Pid,
                Name = d.Name,
                AuthCode = d.AuthCode,
                Component = d.Component,
                Meta = d.Meta,
                Path = d.Path,
                Redirect = d.Redirect,
                Type = d.Type,
                Children = BuildMenuTree(allMenus, d.Id) // 递归处理子节点
            })
            .ToList();
    }

    [OpenApiTag("rbac/menu"), OpenApiOperation("获取用户菜单", "")]
    [RoutePattern(pattern: "all", true, HttpMethod = "GET")]
    public async Task<ApiResult> AllAsync()
    {
        var query = MenuRepository.AsQueryable();
        if (!CurrentUser.Roles.Contains("1")) //1是管理员角色id
        {
            var menuIds = await RoleAuthRepository.AsQueryable()
                .Where(a => CurrentUser.Roles.Select(long.Parse).Contains(a.RoleId))
                .Select(a => a.MenuId)
                .ToListAsync();
            query = query.Where(a => menuIds.Contains(a.Id));
        }
        var allMenus = (await query.ToListAsync()).Map<List<MenuViewDto>>();

        // 构建树形结构
        return ApiResult.Success(BuildMenuTree(allMenus, null));
    }

    [OpenApiTag("rbac/menu"), OpenApiOperation("新增菜单", "")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult> CreateAsync([FromBody] MenuCreateCommand command)
    {
        var entity = command.Map<MenuEntity>();
        await MenuRepository.InsertAsync(entity);
        return ApiResult.Success();
    }

    [OpenApiTag("rbac/menu"), OpenApiOperation("更新菜单", "")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult> UpdateAsync([FromBody] MenuUpdateCommand command)
    {
        var entity = command.Map<MenuEntity>();
        await MenuRepository.UpdateAsync(entity);
        return ApiResult.Success();
    }

    [OpenApiTag("rbac/menu"), OpenApiOperation("删除菜单", "")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult> DeleteAsync(long id)
    {
        await MenuRepository.DeleteAsync(a => id.Equals(a.Id));
        return ApiResult.Success();
    }

    [OpenApiTag("rbac/menu"), OpenApiOperation("批量删除菜单", "")]
    [RoutePattern(pattern: "batchDelete", true, HttpMethod = "Delete")]
    public async Task<ApiResult> BatchDeleteAsync([FromBody] List<long> ids)
    {
        await MenuRepository.DeleteAsync(a => ids.Contains(a.Id));
        return ApiResult.Success();
    }


}
