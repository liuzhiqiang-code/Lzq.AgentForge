using Microsoft.AspNetCore.Builder;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.MES.Domain.Consts;
using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Core.Models;
using Lzq.Extensions.Redis;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SqlSugar;

namespace Lzq.MES.Application.Services;

public class MaterialTypeService : ServiceBase, IMaterialTypeService
{
    public MaterialTypeService() : base("/api/v1/mes/material-type") { }

    private IMaterialTypeRepository Repo => GetRequiredService<IMaterialTypeRepository>();
    private IMaterialRepository MaterialRepo => GetRequiredService<IMaterialRepository>();

    [OpenApiTag("mes/material-type"), OpenApiOperation("分页查询物料类型", "支持按编码、名称、父级筛选")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<MaterialTypeDto>>> PageAsync([FromBody] MaterialTypePageQuery query)
    {
        var expr = Repo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Code), t => t.Code!.Contains(query.Code!))
            .WhereIF(!string.IsNullOrEmpty(query.Name), t => t.Name!.Contains(query.Name!))
            .WhereIF(query.ParentId.HasValue, t => t.ParentId == query.ParentId!.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(t => t.Level).OrderBy(t => t.Sort)
            .ToPageListAsync(query.Page, query.PageSize, total);

        return ApiResult.Success(new PagedResponse<MaterialTypeDto>(list.Map<List<MaterialTypeDto>>(), total));
    }

    [OpenApiTag("mes/material-type"), OpenApiOperation("获取物料类型树", "无限层级树形结构")]
    [RoutePattern(pattern: "tree", true)]
    public async Task<ApiResult<List<MaterialTypeTreeDto>>> TreeAsync()
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var tree = await redis.GetOrSetAsync(RedisKeys.MaterialTypeTree, async () =>
        {
            var all = await Repo.AsQueryable().OrderBy(t => t.Level).OrderBy(t => t.Sort).ToListAsync();
            return BuildTree(all, null);
        }, TimeSpan.FromHours(2));

        return ApiResult.Success(tree ?? new List<MaterialTypeTreeDto>());
    }

    private static List<MaterialTypeTreeDto> BuildTree(List<MaterialTypeEntity> all, long? parentId)
    {
        return all.Where(t => t.ParentId == parentId).Select(t => new MaterialTypeTreeDto
        {
            Id = t.Id,
            Code = t.Code,
            Name = t.Name,
            Level = t.Level,
            Children = BuildTree(all, t.Id),
        }).ToList();
    }

    [OpenApiTag("mes/material-type"), OpenApiOperation("创建物料类型")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] MaterialTypeCreateCommand command)
    {
        var exists = await Repo.AsQueryable().AnyAsync(t => t.Code == command.Code);
        if (exists) throw new UserFriendlyException($"物料类型编码 [{command.Code}] 已存在");

        var level = 1;
        if (command.ParentId.HasValue)
        {
            var parent = await Repo.GetByIdAsync(command.ParentId.Value)
                ?? throw new UserFriendlyException("父级物料类型不存在");
            level = parent.Level + 1;
        }

        var entity = command.Map<MaterialTypeEntity>();
        entity.Level = level;
        await Repo.InsertAsync(entity);

        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.MaterialTypeTree);

        return ApiResult.Success(entity.Id);
    }

    [OpenApiTag("mes/material-type"), OpenApiOperation("更新物料类型")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult<bool>> UpdateAsync([FromBody] MaterialTypeUpdateCommand command)
    {
        var entity = await Repo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("物料类型不存在");

        var codeExists = await Repo.AsQueryable().AnyAsync(t => t.Code == command.Code && t.Id != command.Id);
        if (codeExists) throw new UserFriendlyException($"物料类型编码 [{command.Code}] 已被使用");

        var level = 1;
        if (command.ParentId.HasValue)
        {
            var parent = await Repo.GetByIdAsync(command.ParentId.Value)
                ?? throw new UserFriendlyException("父级物料类型不存在");
            level = parent.Level + 1;
        }

        command.Map(entity);
        entity.Level = level;
        await Repo.UpdateAsync(entity);

        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.MaterialTypeTree);

        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/material-type"), OpenApiOperation("删除物料类型", "有子节点或引用物料不允许删除")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("物料类型不存在");

        var hasChildren = await Repo.AsQueryable().AnyAsync(t => t.ParentId == id);
        if (hasChildren) throw new UserFriendlyException("存在子节点，不允许删除");

        var hasMaterials = await MaterialRepo.AsQueryable().AnyAsync(m => m.MaterialTypeId == id);
        if (hasMaterials) throw new UserFriendlyException("该分类下存在物料，不允许删除");

        await Repo.DeleteAsync(entity);

        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.MaterialTypeTree);

        return ApiResult.Success(true);
    }
}
