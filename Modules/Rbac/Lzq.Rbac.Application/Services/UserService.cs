using Lzq.Core.Models;
using Lzq.Rbac.Application.Contracts.IServices;
using Lzq.Rbac.Domain.Entities;
using Lzq.Rbac.Domain.IRepositories;
using Lzq.Rbac.Domain.Consts;
using Lzq.Extensions.Redis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SqlSugar;
using Lzq.Rbac.Application.Contracts.Commands;
using Lzq.Rbac.Application.Contracts.Queries;
using Lzq.Rbac.Application.Contracts.Dtos;

/*
 * @author : liuzhiqiang
 * @date : 2026-1-1
 * @desc : user
 */
namespace Lzq.Rbac.Application.Services;

public class UserService : ServiceBase, IUserService
{
    public UserService() : base("/api/v1/rbac/user") { }
    private IUserRepository UserRepository => GetRequiredService<IUserRepository>();

    [OpenApiTag("rbac/user"), OpenApiOperation("获取用户分页列表", "")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult> PageAsync([FromBody] UserPageQuery query)
    {
        RefAsync<int> total = 0;
        var pageList = await UserRepository.AsQueryable().ToPageListAsync(query.Page, query.PageSize, total);
        var result = pageList.Map<List<UserViewDto>>();
        return ApiResult.Success(new PagedResponse<UserViewDto>(result, total));
    }

    [OpenApiTag("rbac/user"), OpenApiOperation("获取用户列表", "")]
    [RoutePattern(pattern: "list", true)]
    public async Task<ApiResult> ListAsync()
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var cacheKey = RedisKeys.UserList;
        
        var list = await redis.GetOrSetAsync(cacheKey, async () =>
        {
            var result = (await UserRepository.GetListAsync()).ToList();
            return result.Map<List<UserViewDto>>();
        }, TimeSpan.FromHours(1)); // 用户列表缓存1小时

        return ApiResult.Success(list ?? new List<UserViewDto>());
    }

    [OpenApiTag("rbac/user"), OpenApiOperation("增加用户", "")]
    [RoutePattern(pattern: "create", true)]
    [AllowAnonymous]
    public async Task<ApiResult> CreateAsync([FromBody] UserCreateCommand command)
    {
        var entity = command.Map<UserEntity>();
        await UserRepository.InsertAsync(entity);
        
        // 清除用户列表缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.UserList);
        
        return ApiResult.Success();
    }

    [OpenApiTag("rbac/user"), OpenApiOperation("更新用户", "")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult> UpdateAsync([FromBody] UserUpdateCommand command)
    {
        var entity = command.Map<UserEntity>();
        await UserRepository.UpdateAsync(entity);
        
        // 清除用户列表缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.UserList);
        
        return ApiResult.Success();
    }

    [OpenApiTag("rbac/user"), OpenApiOperation("删除用户", "")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult> DeleteAsync(long id)
    {
        await UserRepository.DeleteAsync(a => id.Equals(a.Id));
        
        // 清除用户列表缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.UserList);
        
        return ApiResult.Success();
    }

    [OpenApiTag("rbac/user"), OpenApiOperation("批量删除用户", "")]
    [RoutePattern(pattern: "batchDelete", true, HttpMethod = "Delete")]
    public async Task<ApiResult> BatchDeleteAsync([FromBody] List<long> ids)
    {
        await UserRepository.DeleteAsync(a => ids.Contains(a.Id));
        
        // 清除用户列表缓存
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.UserList);
        
        return ApiResult.Success();
    }
}