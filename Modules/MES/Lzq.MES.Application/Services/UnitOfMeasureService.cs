using Microsoft.AspNetCore.Builder;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.MES.Domain.Consts;
using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.Enums;
using Lzq.MES.Domain.IRepositories;
using Lzq.Core.Models;
using Lzq.Extensions.Redis;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SqlSugar;

namespace Lzq.MES.Application.Services;

public class UnitOfMeasureService : ServiceBase, IUnitOfMeasureService
{
    public UnitOfMeasureService() : base("/api/v1/mes/unit-of-measure") { }

    private IUnitOfMeasureRepository Repo => GetRequiredService<IUnitOfMeasureRepository>();

    [OpenApiTag("mes/unit-of-measure"), OpenApiOperation("分页查询计量单位", "支持按编码、名称、分类筛选")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<UnitOfMeasureDto>>> PageAsync([FromBody] UnitOfMeasurePageQuery query)
    {
        var expr = Repo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Code), u => u.Code!.Contains(query.Code!))
            .WhereIF(!string.IsNullOrEmpty(query.Name), u => u.Name!.Contains(query.Name!))
            .WhereIF(query.Category.HasValue, u => u.Category == (UnitCategoryEnum)query.Category!.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(u => u.Code)
            .ToPageListAsync(query.Page, query.PageSize, total);

        return ApiResult.Success(new PagedResponse<UnitOfMeasureDto>(list.Map<List<UnitOfMeasureDto>>(), total));
    }

    [OpenApiTag("mes/unit-of-measure"), OpenApiOperation("获取计量单位下拉列表", "缓存 2 小时")]
    [RoutePattern(pattern: "select-list", true)]
    public async Task<ApiResult<List<UnitOfMeasureDto>>> SelectListAsync()
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var list = await redis.GetOrSetAsync(RedisKeys.UnitOfMeasureList, async () =>
        {
            var entities = await Repo.AsQueryable().OrderBy(u => u.Code).ToListAsync();
            return entities.Map<List<UnitOfMeasureDto>>();
        }, TimeSpan.FromHours(2));

        return ApiResult.Success(list ?? new List<UnitOfMeasureDto>());
    }

    [OpenApiTag("mes/unit-of-measure"), OpenApiOperation("创建计量单位", "编码不可重复")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] UnitOfMeasureCreateCommand command)
    {
        var exists = await Repo.AsQueryable().AnyAsync(u => u.Code == command.Code);
        if (exists) throw new UserFriendlyException($"计量单位编码 [{command.Code}] 已存在");

        var entity = command.Map<UnitOfMeasureEntity>();
        await Repo.InsertAsync(entity);

        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.UnitOfMeasureList);

        return ApiResult.Success(entity.Id);
    }

    [OpenApiTag("mes/unit-of-measure"), OpenApiOperation("更新计量单位")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult<bool>> UpdateAsync([FromBody] UnitOfMeasureUpdateCommand command)
    {
        var entity = await Repo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("计量单位不存在");

        var codeExists = await Repo.AsQueryable().AnyAsync(u => u.Code == command.Code && u.Id != command.Id);
        if (codeExists) throw new UserFriendlyException($"计量单位编码 [{command.Code}] 已被使用");

        command.Map(entity);
        await Repo.UpdateAsync(entity);

        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.UnitOfMeasureList);

        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/unit-of-measure"), OpenApiOperation("删除计量单位")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("计量单位不存在");
        await Repo.DeleteAsync(entity);

        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.UnitOfMeasureList);

        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/unit-of-measure"), OpenApiOperation("批量删除计量单位")]
    [RoutePattern(pattern: "batch-delete", true)]
    public async Task<ApiResult<int>> BatchDeleteAsync([FromBody] List<long> ids)
    {
        if (ids is null || ids.Count == 0) throw new UserFriendlyException("请选择要删除的计量单位");
        await Repo.DeleteAsync(u => ids.Contains(u.Id));

        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.UnitOfMeasureList);

        return ApiResult.Success(ids.Count);
    }
}
