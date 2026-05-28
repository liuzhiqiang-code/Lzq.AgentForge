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

public class MaterialService : ServiceBase, IMaterialService
{
    public MaterialService() : base("/api/v1/mes/material") { }

    private IMaterialRepository Repo => GetRequiredService<IMaterialRepository>();
    private IMaterialTypeRepository MaterialTypeRepo => GetRequiredService<IMaterialTypeRepository>();
    private IUnitOfMeasureRepository UnitOfMeasureRepo => GetRequiredService<IUnitOfMeasureRepository>();
    private IBomRepository BomRepo => GetRequiredService<IBomRepository>();
    private IBomItemRepository BomItemRepo => GetRequiredService<IBomItemRepository>();

    [OpenApiTag("mes/material"), OpenApiOperation("分页查询物料", "支持编码、名称、规格、类型、状态筛选")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<MaterialDto>>> PageAsync([FromBody] MaterialPageQuery query)
    {
        var expr = Repo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Code), m => m.Code!.Contains(query.Code!))
            .WhereIF(!string.IsNullOrEmpty(query.Name), m => m.Name!.Contains(query.Name!))
            .WhereIF(!string.IsNullOrEmpty(query.Spec), m => m.Spec!.Contains(query.Spec!))
            .WhereIF(query.MaterialTypeId.HasValue, m => m.MaterialTypeId == query.MaterialTypeId!.Value)
            .WhereIF(query.UnitId.HasValue, m => m.UnitId == query.UnitId!.Value)
            .WhereIF(query.Status.HasValue, m => m.Status == (MaterialStatusEnum)query.Status!.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(m => m.Code)
            .ToPageListAsync(query.Page, query.PageSize, total);

        var dtos = list.Map<List<MaterialDto>>();

        var typeIds = dtos.Where(d => d.MaterialTypeId > 0).Select(d => d.MaterialTypeId).Distinct().ToList();
        var unitIds = dtos.Where(d => d.UnitId > 0).Select(d => d.UnitId).Distinct().ToList();

        var types = new Dictionary<long, string>();
        if (typeIds.Count > 0)
        {
            var typeDict = await MaterialTypeRepo.AsQueryable()
                .Where(t => typeIds.Contains(t.Id))
                .ToListAsync();
            types = typeDict.ToDictionary(t => t.Id, t => t.Name);
        }

        var units = new Dictionary<long, string>();
        if (unitIds.Count > 0)
        {
            var unitDict = await UnitOfMeasureRepo.AsQueryable()
                .Where(u => unitIds.Contains(u.Id))
                .ToListAsync();
            units = unitDict.ToDictionary(u => u.Id, u => u.Name);
        }

        foreach (var dto in dtos)
        {
            types.TryGetValue(dto.MaterialTypeId, out var tn);
            dto.MaterialTypeName = tn;
            units.TryGetValue(dto.UnitId, out var un);
            dto.UnitName = un;
        }

        return ApiResult.Success(new PagedResponse<MaterialDto>(dtos, total));
    }

    [OpenApiTag("mes/material"), OpenApiOperation("获取物料详情")]
    [RoutePattern(pattern: "get/{id}", true)]
    public async Task<ApiResult<MaterialDto>> GetAsync(long id)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("物料不存在");
        var dto = entity.Map<MaterialDto>();

        var typeName = await MaterialTypeRepo.AsQueryable()
            .Where(t => t.Id == entity.MaterialTypeId).Select(t => t.Name).FirstAsync();
        dto.MaterialTypeName = typeName;

        var unitName = await UnitOfMeasureRepo.AsQueryable()
            .Where(u => u.Id == entity.UnitId).Select(u => u.Name).FirstAsync();
        dto.UnitName = unitName;

        return ApiResult.Success(dto);
    }

    [OpenApiTag("mes/material"), OpenApiOperation("获取物料下拉列表", "缓存 2 小时")]
    [RoutePattern(pattern: "select-list", true)]
    public async Task<ApiResult<List<MaterialDto>>> SelectListAsync()
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var list = await redis.GetOrSetAsync(RedisKeys.MaterialSelectList, async () =>
        {
            var entities = await Repo.AsQueryable()
                .Where(m => m.Status == MaterialStatusEnum.Enabled)
                .OrderBy(m => m.Code).ToListAsync();
            return entities.Map<List<MaterialDto>>();
        }, TimeSpan.FromHours(2));

        return ApiResult.Success(list ?? new List<MaterialDto>());
    }

    [OpenApiTag("mes/material"), OpenApiOperation("创建物料", "编码不可重复")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] MaterialCreateCommand command)
    {
        var exists = await Repo.AsQueryable().AnyAsync(m => m.Code == command.Code);
        if (exists) throw new UserFriendlyException($"物料编码 [{command.Code}] 已存在");

        var typeExists = await MaterialTypeRepo.AsQueryable().AnyAsync(t => t.Id == command.MaterialTypeId);
        if (!typeExists) throw new UserFriendlyException("物料类型不存在");

        var unitExists = await UnitOfMeasureRepo.AsQueryable().AnyAsync(u => u.Id == command.UnitId);
        if (!unitExists) throw new UserFriendlyException("计量单位不存在");

        var entity = command.Map<MaterialEntity>();
        await Repo.InsertAsync(entity);

        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.MaterialSelectList);

        return ApiResult.Success(entity.Id);
    }

    [OpenApiTag("mes/material"), OpenApiOperation("更新物料", "含状态变更BOM联动")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult<bool>> UpdateAsync([FromBody] MaterialUpdateCommand command)
    {
        var entity = await Repo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("物料不存在");

        var codeExists = await Repo.AsQueryable().AnyAsync(m => m.Code == command.Code && m.Id != command.Id);
        if (codeExists) throw new UserFriendlyException($"物料编码 [{command.Code}] 已被使用");

        var oldStatus = entity.Status;
        command.Map(entity);

        if (oldStatus != entity.Status && entity.Status is MaterialStatusEnum.Disabled or MaterialStatusEnum.Obsolete)
        {
            await MarkRelatedBomsRevisionPendingAsync(entity);
        }

        await Repo.UpdateAsync(entity);

        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.MaterialSelectList);

        return ApiResult.Success(true);
    }

    private async Task MarkRelatedBomsRevisionPendingAsync(MaterialEntity material)
    {
        var bomItemRefs = await BomItemRepo.AsQueryable()
            .Where(i => i.ItemId == material.Id)
            .Select(i => i.BomId)
            .ToListAsync();

        var bomProductRefs = await BomRepo.AsQueryable()
            .Where(b => b.ProductId == material.Id)
            .Select(b => b.Id)
            .ToListAsync();

        var affectedBomIds = bomItemRefs.Concat(bomProductRefs).Distinct().ToList();
        if (affectedBomIds.Count == 0) return;

        var affectedBoms = await BomRepo.AsQueryable()
            .Where(b => affectedBomIds.Contains(b.Id) && b.Status == BomStatusEnum.Released)
            .ToListAsync();

        foreach (var bom in affectedBoms)
        {
            bom.Status = BomStatusEnum.RevisionPending;
            await BomRepo.UpdateAsync(bom);
        }
    }

    [OpenApiTag("mes/material"), OpenApiOperation("删除物料", "检查 BOM 引用")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("物料不存在");

        var references = await GetReferenceInfoAsync(id);
        if (references.Count > 0)
            throw new UserFriendlyException($"物料 [{entity.Code}] 被以下BOM引用，无法删除：{string.Join("、", references)}");

        await Repo.DeleteAsync(entity);

        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.MaterialSelectList);

        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/material"), OpenApiOperation("批量删除物料", "检查 BOM 引用")]
    [RoutePattern(pattern: "batch-delete", true)]
    public async Task<ApiResult<int>> BatchDeleteAsync([FromBody] List<long> ids)
    {
        if (ids is null || ids.Count == 0) throw new UserFriendlyException("请选择要删除的物料");

        var allRefs = new List<string>();
        foreach (var id in ids)
        {
            var refs = await GetReferenceInfoAsync(id);
            if (refs.Count > 0)
            {
                var material = await Repo.GetByIdAsync(id);
                allRefs.Add($"物料 [{material?.Code ?? id.ToString()}]: {string.Join("、", refs)}");
            }
        }

        if (allRefs.Count > 0)
            throw new UserFriendlyException("以下物料存在 BOM 引用，无法删除：\n" + string.Join("\n", allRefs));

        await Repo.DeleteAsync(m => ids.Contains(m.Id));

        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.MaterialSelectList);

        return ApiResult.Success(ids.Count);
    }

    private async Task<List<string>> GetReferenceInfoAsync(long materialId)
    {
        var refs = new List<string>();

        var bomItems = await BomItemRepo.AsQueryable()
            .Where(i => i.ItemId == materialId)
            .Select(i => i.BomId)
            .ToListAsync();

        if (bomItems.Count > 0)
        {
            var bomCodes = await BomRepo.AsQueryable()
                .Where(b => bomItems.Contains(b.Id))
                .Select(b => b.Code)
                .ToListAsync();
            refs.AddRange(bomCodes.Select(c => $"BOM[{c}]作为子物料"));
        }

        var productBoms = await BomRepo.AsQueryable()
            .Where(b => b.ProductId == materialId)
            .Select(b => b.Code)
            .ToListAsync();

        refs.AddRange(productBoms.Select(c => $"BOM[{c}]作为母件"));

        return refs;
    }
}
