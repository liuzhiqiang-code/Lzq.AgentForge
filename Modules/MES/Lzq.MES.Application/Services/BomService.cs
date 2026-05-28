using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.Enums;
using Lzq.MES.Domain.IRepositories;
using Lzq.Core.Models;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SqlSugar;

namespace Lzq.MES.Application.Services;

public class BomService : ServiceBase, IBomService
{
    public BomService() : base("/api/v1/mes/bom") { }

    private IBomRepository Repo => GetRequiredService<IBomRepository>();
    private IBomItemRepository ItemRepo => GetRequiredService<IBomItemRepository>();
    private IMaterialRepository MaterialRepo => GetRequiredService<IMaterialRepository>();
    private IBomVersionHistoryRepository HistoryRepo => GetRequiredService<IBomVersionHistoryRepository>();

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    // ==================== BOM 头====================

    [OpenApiTag("mes/bom"), OpenApiOperation("分页查询 BOM", "支持按编码、名称、产品、状态筛选")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<BomDto>>> PageAsync([FromBody] BomPageQuery query)
    {
        var expr = Repo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Code), b => b.Code!.Contains(query.Code!))
            .WhereIF(!string.IsNullOrEmpty(query.Name), b => b.Name!.Contains(query.Name!))
            .WhereIF(query.ProductId.HasValue, b => b.ProductId == query.ProductId!.Value)
            .WhereIF(query.Status.HasValue, b => b.Status == (BomStatusEnum)query.Status!.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(b => b.Code)
            .ToPageListAsync(query.Page, query.PageSize, total);

        var dtos = list.Map<List<BomDto>>();
        var productIds = dtos.Where(d => d.ProductId > 0).Select(d => d.ProductId).Distinct().ToList();

        if (productIds.Count > 0)
        {
            var productList = await MaterialRepo.AsQueryable()
                .Where(m => productIds.Contains(m.Id))
                .ToListAsync();
            var products = productList.ToDictionary(m => m.Id);

            foreach (var dto in dtos)
            {
                if (products.TryGetValue(dto.ProductId, out var p))
                {
                    dto.ProductName = p.Name;
                    dto.ProductCode = p.Code;
                }
            }
        }

        return ApiResult.Success(new PagedResponse<BomDto>(dtos, total));
    }

    [OpenApiTag("mes/bom"), OpenApiOperation("获取 BOM 详情", "含 BOM 头和明细")]
    [RoutePattern(pattern: "get/{id}", true)]
    public async Task<ApiResult<BomDetailDto>> GetAsync(long id)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("BOM 不存在");

        var header = entity.Map<BomDto>();
        var product = await MaterialRepo.GetByIdAsync(entity.ProductId);
        if (product is not null)
        {
            header.ProductName = product.Name;
            header.ProductCode = product.Code;
        }

        var items = await ItemRepo.AsQueryable()
            .Where(i => i.BomId == id)
            .OrderBy(i => i.Sort)
            .ToListAsync();

        var itemDtos = items.Map<List<BomItemDto>>();
        var materialIds = itemDtos.Where(d => d.ItemId > 0).Select(d => d.ItemId).Distinct().ToList();

        if (materialIds.Count > 0)
        {
            var materialList = await MaterialRepo.AsQueryable()
                .Where(m => materialIds.Contains(m.Id))
                .ToListAsync();
            var materials = materialList.ToDictionary(m => m.Id);

            foreach (var dto in itemDtos)
            {
                if (materials.TryGetValue(dto.ItemId, out var mat))
                {
                    dto.ItemCode = mat.Code;
                    dto.ItemName = mat.Name;
                    dto.ItemSpec = mat.Spec;
                }
            }
        }

        return ApiResult.Success(new BomDetailDto { Header = header, Items = itemDtos });
    }

    [OpenApiTag("mes/bom"), OpenApiOperation("创建 BOM")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] BomCreateCommand command)
    {
        var exists = await Repo.AsQueryable().AnyAsync(b => b.Code == command.Code);
        if (exists) throw new UserFriendlyException($"BOM 编码 [{command.Code}] 已存在");

        var product = await MaterialRepo.GetByIdAsync(command.ProductId);
        if (product is null) throw new UserFriendlyException("产品物料不存在");

        var entity = command.Map<BomEntity>();
        entity.Status = BomStatusEnum.Draft;
        await Repo.InsertAsync(entity);

        return ApiResult.Success(entity.Id);
    }

    [OpenApiTag("mes/bom"), OpenApiOperation("更新 BOM")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult<bool>> UpdateAsync([FromBody] BomUpdateCommand command)
    {
        var entity = await Repo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("BOM 不存在");

        if (entity.Status is BomStatusEnum.Released or BomStatusEnum.RevisionPending)
            throw new UserFriendlyException("已发布或待修订的 BOM 不允许修改，请先复制新版本");

        var codeExists = await Repo.AsQueryable().AnyAsync(b => b.Code == command.Code && b.Id != command.Id);
        if (codeExists) throw new UserFriendlyException($"BOM 编码 [{command.Code}] 已被使用");

        command.Map(entity);
        await Repo.UpdateAsync(entity);

        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/bom"), OpenApiOperation("删除 BOM")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("BOM 不存在");

        if (entity.Status is BomStatusEnum.Released or BomStatusEnum.RevisionPending)
            throw new UserFriendlyException("已发布或待修订的 BOM 不允许删除，请先废弃");

        await ItemRepo.DeleteAsync(i => i.BomId == id);
        await HistoryRepo.DeleteAsync(h => h.BomId == id);
        await Repo.DeleteAsync(entity);

        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/bom"), OpenApiOperation("复制 BOM", "复制一份草稿，版本+1")]
    [RoutePattern(pattern: "copy/{id}", true)]
    public async Task<ApiResult<long>> CopyAsync(long id)
    {
        var source = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("源BOM 不存在");

        var newEntity = source.Map<BomEntity>();
        newEntity.Id = 0;
        newEntity.Status = BomStatusEnum.Draft;
        newEntity.Version = BumpVersion(source.Version);

        var copySuffix = 1;
        var newCode = $"{source.Code}-COPY";
        while (await Repo.AsQueryable().AnyAsync(b => b.Code == newCode))
        {
            copySuffix++;
            newCode = $"{source.Code}-COPY{copySuffix}";
        }
        newEntity.Code = newCode;
        newEntity.Name = $"{source.Name}(复制)";

        await Repo.InsertAsync(newEntity);

        var items = await ItemRepo.AsQueryable().Where(i => i.BomId == source.Id).ToListAsync();
        foreach (var item in items)
        {
            item.Id = 0;
            item.BomId = newEntity.Id;
        }
        if (items.Count > 0)
            await ItemRepo.InsertRangeAsync(items);

        return ApiResult.Success(newEntity.Id);
    }

    [OpenApiTag("mes/bom"), OpenApiOperation("发布 BOM", "同一产品同一时间只有一个发布 BOM，同时生成版本快照")]
    [RoutePattern(pattern: "release/{id}", true)]
    public async Task<ApiResult<bool>> ReleaseAsync(long id, string? changeDescription = null)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("BOM 不存在");

        if (entity.Status != BomStatusEnum.Draft)
            throw new UserFriendlyException("只有草稿状态的 BOM 可以发布");

        var released = await Repo.AsQueryable()
            .Where(b => b.ProductId == entity.ProductId && b.Status == BomStatusEnum.Released && b.Id != id)
            .ToListAsync();

        foreach (var b in released)
        {
            b.Status = BomStatusEnum.Obsolete;
            await Repo.UpdateAsync(b);
        }

        entity.Status = BomStatusEnum.Released;
        entity.EffDate ??= DateTime.Now;
        await Repo.UpdateAsync(entity);

        await SnapshotVersionAsync(entity, changeDescription);

        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/bom"), OpenApiOperation("回滚 BOM", "将 BOM 恢复到指定历史版本的快照")]
    [RoutePattern(pattern: "rollback/{bomId}/{historyId}", true)]
    public async Task<ApiResult<bool>> RollbackAsync(long bomId, long historyId)
    {
        var entity = await Repo.GetByIdAsync(bomId)
            ?? throw new UserFriendlyException("BOM 不存在");

        var history = await HistoryRepo.GetByIdAsync(historyId)
            ?? throw new UserFriendlyException("版本历史不存在");

        if (history.BomId != bomId)
            throw new UserFriendlyException("版本历史不属于该 BOM");

        var snapshot = JsonSerializer.Deserialize<BomSnapshot>(history.SnapshotJson, _jsonOpts)
            ?? throw new UserFriendlyException("版本快照数据异常");

        entity.Code = snapshot.Code;
        entity.Name = snapshot.Name;
        entity.ProductId = snapshot.ProductId;
        entity.Version = BumpVersion(entity.Version);
        entity.EffDate = snapshot.EffDate;
        entity.ExpDate = snapshot.ExpDate;
        entity.Remark = snapshot.Remark;
        entity.Status = BomStatusEnum.Draft;
        await Repo.UpdateAsync(entity);

        await ItemRepo.DeleteAsync(i => i.BomId == bomId);

        foreach (var itemSnapshot in snapshot.Items)
        {
            var item = new BomItemEntity
            {
                BomId = bomId,
                ItemId = itemSnapshot.ItemId,
                Qty = itemSnapshot.Qty,
                ScrapRate = itemSnapshot.ScrapRate,
                Sort = itemSnapshot.Sort,
                SubstituteIds = itemSnapshot.SubstituteIds,
                Remark = itemSnapshot.Remark,
            };
            await ItemRepo.InsertAsync(item);
        }

        return ApiResult.Success(true);
    }

    // ==================== BOM 明细 ====================

    [OpenApiTag("mes/bom"), OpenApiOperation("获取 BOM 明细列表")]
    [RoutePattern(pattern: "items/{bomId}", true)]
    public async Task<ApiResult<List<BomItemDto>>> GetItemsAsync(long bomId)
    {
        var bomExists = await Repo.AsQueryable().AnyAsync(b => b.Id == bomId);
        if (!bomExists) throw new UserFriendlyException("BOM 不存在");

        var items = await ItemRepo.AsQueryable()
            .Where(i => i.BomId == bomId)
            .OrderBy(i => i.Sort)
            .ToListAsync();

        var dtos = items.Map<List<BomItemDto>>();
        var materialIds = dtos.Where(d => d.ItemId > 0).Select(d => d.ItemId).Distinct().ToList();

        if (materialIds.Count > 0)
        {
            var materialList = await MaterialRepo.AsQueryable()
                .Where(m => materialIds.Contains(m.Id))
                .ToListAsync();
            var materials = materialList.ToDictionary(m => m.Id);

            foreach (var dto in dtos)
            {
                if (materials.TryGetValue(dto.ItemId, out var mat))
                {
                    dto.ItemCode = mat.Code;
                    dto.ItemName = mat.Name;
                    dto.ItemSpec = mat.Spec;
                }
            }
        }

        return ApiResult.Success(dtos);
    }

    [OpenApiTag("mes/bom"), OpenApiOperation("添加 BOM 明细")]
    [RoutePattern(pattern: "item/create", true)]
    public async Task<ApiResult<long>> CreateItemAsync([FromBody] BomItemCreateCommand command)
    {
        var bom = await Repo.GetByIdAsync(command.BomId)
            ?? throw new UserFriendlyException("BOM 不存在");

        if (bom.Status != BomStatusEnum.Draft)
            throw new UserFriendlyException("只有草稿状态的 BOM 允许修改明细");

        var materialExists = await MaterialRepo.AsQueryable().AnyAsync(m => m.Id == command.ItemId);
        if (!materialExists) throw new UserFriendlyException("子物料不存在");

        if (command.ItemId == bom.ProductId)
            throw new UserFriendlyException("子物料不能与母件相同");

        var entity = command.Map<BomItemEntity>();
        await ItemRepo.InsertAsync(entity);

        return ApiResult.Success(entity.Id);
    }

    [OpenApiTag("mes/bom"), OpenApiOperation("更新 BOM 明细")]
    [RoutePattern(pattern: "item/update", true)]
    public async Task<ApiResult<bool>> UpdateItemAsync([FromBody] BomItemUpdateCommand command)
    {
        var entity = await ItemRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("BOM 明细不存在");

        var bom = await Repo.GetByIdAsync(entity.BomId);
        if (bom?.Status != BomStatusEnum.Draft)
            throw new UserFriendlyException("只有草稿状态的 BOM 允许修改明细");

        command.Map(entity);
        await ItemRepo.UpdateAsync(entity);

        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/bom"), OpenApiOperation("删除 BOM 明细")]
    [RoutePattern(pattern: "item/delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteItemAsync(long id)
    {
        var entity = await ItemRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("BOM 明细不存在");

        var bom = await Repo.GetByIdAsync(entity.BomId);
        if (bom?.Status != BomStatusEnum.Draft)
            throw new UserFriendlyException("只有草稿状态的 BOM 允许修改明细");

        await ItemRepo.DeleteAsync(entity);

        return ApiResult.Success(true);
    }

    // ==================== 版本历史 ====================

    [OpenApiTag("mes/bom"), OpenApiOperation("获取 BOM 版本历史列表")]
    [RoutePattern(pattern: "version-history/{bomId}", true)]
    public async Task<ApiResult<List<BomVersionHistoryDto>>> GetVersionHistoryAsync(long bomId)
    {
        var bomExists = await Repo.AsQueryable().AnyAsync(b => b.Id == bomId);
        if (!bomExists) throw new UserFriendlyException("BOM 不存在");

        var list = await HistoryRepo.AsQueryable()
            .Where(h => h.BomId == bomId)
            .OrderByDescending(h => h.CreationTime)
            .ToListAsync();

        return ApiResult.Success(list.Map<List<BomVersionHistoryDto>>());
    }

    [OpenApiTag("mes/bom"), OpenApiOperation("版本对比", "对比两个历史版本的差异")]
    [RoutePattern(pattern: "diff/{historyId1}/{historyId2}", true)]
    public async Task<ApiResult<BomDiffResultDto>> DiffVersionsAsync(long historyId1, long historyId2)
    {
        var h1 = await HistoryRepo.GetByIdAsync(historyId1)
            ?? throw new UserFriendlyException("版本历史 1 不存在");
        var h2 = await HistoryRepo.GetByIdAsync(historyId2)
            ?? throw new UserFriendlyException("版本历史 2 不存在");

        var s1 = JsonSerializer.Deserialize<BomSnapshot>(h1.SnapshotJson, _jsonOpts)!;
        var s2 = JsonSerializer.Deserialize<BomSnapshot>(h2.SnapshotJson, _jsonOpts)!;

        var result = new BomDiffResultDto
        {
            OldVersion = h1.Version,
            NewVersion = h2.Version,
            HeaderDiff = GenerateHeaderDiff(s1, s2),
        };

        var oldItems = s1.Items.ToDictionary(i => i.ItemId);
        var newItems = s2.Items.ToDictionary(i => i.ItemId);

        var allItemIds = oldItems.Keys.Union(newItems.Keys).ToList();
        foreach (var itemId in allItemIds)
        {
            var hasOld = oldItems.TryGetValue(itemId, out var oldItem);
            var hasNew = newItems.TryGetValue(itemId, out var newItem);

            if (hasOld && !hasNew)
            {
                result.ItemChanges.Add(new BomItemDiffDto { ChangeType = "删除", ItemId = itemId });
            }
            else if (!hasOld && hasNew)
            {
                result.ItemChanges.Add(new BomItemDiffDto
                {
                    ChangeType = "新增",
                    ItemId = itemId,
                    NewQty = newItem!.Qty,
                    NewScrapRate = newItem.ScrapRate,
                });
            }
            else if (oldItem!.Qty != newItem!.Qty || oldItem.ScrapRate != newItem.ScrapRate)
            {
                result.ItemChanges.Add(new BomItemDiffDto
                {
                    ChangeType = "修改",
                    ItemId = itemId,
                    OldQty = oldItem.Qty,
                    NewQty = newItem.Qty,
                    OldScrapRate = oldItem.ScrapRate,
                    NewScrapRate = newItem.ScrapRate,
                });
            }
        }

        return ApiResult.Success(result);
    }

    [OpenApiTag("mes/bom"), OpenApiOperation("预览历史版本", "查看指定历史版本的BOM 内容")]
    [RoutePattern(pattern: "preview-version/{historyId}", true)]
    public async Task<ApiResult<BomDetailDto>> PreviewVersionAsync(long historyId)
    {
        var history = await HistoryRepo.GetByIdAsync(historyId)
            ?? throw new UserFriendlyException("版本历史不存在");

        var snapshot = JsonSerializer.Deserialize<BomSnapshot>(history.SnapshotJson, _jsonOpts)
            ?? throw new UserFriendlyException("版本快照数据异常");

        var header = new BomDto
        {
            Id = history.BomId,
            Code = snapshot.Code,
            Name = snapshot.Name,
            ProductId = snapshot.ProductId,
            Version = snapshot.Version,
            EffDate = snapshot.EffDate,
            ExpDate = snapshot.ExpDate,
            Remark = snapshot.Remark,
        };

        var product = await MaterialRepo.GetByIdAsync(snapshot.ProductId);
        if (product is not null)
        {
            header.ProductName = product.Name;
            header.ProductCode = product.Code;
        }

        var items = snapshot.Items.Select(i => new BomItemDto
        {
            ItemId = i.ItemId,
            Qty = i.Qty,
            ScrapRate = i.ScrapRate,
            Sort = i.Sort,
            SubstituteIds = i.SubstituteIds,
            Remark = i.Remark,
        }).ToList();

        var materialIds = items.Where(d => d.ItemId > 0).Select(d => d.ItemId).Distinct().ToList();
        if (materialIds.Count > 0)
        {
            var materials = await MaterialRepo.AsQueryable()
                .Where(m => materialIds.Contains(m.Id))
                .ToListAsync();
            var matDict = materials.ToDictionary(m => m.Id);
            foreach (var dto in items)
            {
                if (matDict.TryGetValue(dto.ItemId, out var mat))
                {
                    dto.ItemCode = mat.Code;
                    dto.ItemName = mat.Name;
                    dto.ItemSpec = mat.Spec;
                }
            }
        }

        return ApiResult.Success(new BomDetailDto { Header = header, Items = items });
    }

    // ==================== 私有方法 ====================

    private async Task SnapshotVersionAsync(BomEntity bom, string? changeDescription)
    {
        var items = await ItemRepo.AsQueryable()
            .Where(i => i.BomId == bom.Id)
            .OrderBy(i => i.Sort)
            .ToListAsync();

        var snapshot = new BomSnapshot
        {
            Code = bom.Code,
            Name = bom.Name,
            ProductId = bom.ProductId,
            Version = bom.Version,
            EffDate = bom.EffDate,
            ExpDate = bom.ExpDate,
            Remark = bom.Remark,
            Items = items.Select(i => new BomItemSnapshot
            {
                ItemId = i.ItemId,
                Qty = i.Qty,
                ScrapRate = i.ScrapRate,
                Sort = i.Sort,
                SubstituteIds = i.SubstituteIds,
                Remark = i.Remark,
            }).ToList(),
        };

        var history = new BomVersionHistoryEntity
        {
            BomId = bom.Id,
            Version = bom.Version,
            SnapshotJson = JsonSerializer.Serialize(snapshot, _jsonOpts),
            ChangeDescription = changeDescription,
            ProductId = bom.ProductId,
        };

        await HistoryRepo.InsertAsync(history);
    }

    private static string GenerateHeaderDiff(BomSnapshot s1, BomSnapshot s2)
    {
        var diffs = new List<string>();
        if (s1.Code != s2.Code) diffs.Add($"编码: {s1.Code} →{s2.Code}");
        if (s1.Name != s2.Name) diffs.Add($"名称: {s1.Name} →{s2.Name}");
        if (s1.ProductId != s2.ProductId) diffs.Add("母件变更");
        if (s1.EffDate != s2.EffDate) diffs.Add($"生效日期: {s1.EffDate} →{s2.EffDate}");
        if (s1.ExpDate != s2.ExpDate) diffs.Add($"失效日期: {s1.ExpDate} →{s2.ExpDate}");
        if (s1.Remark != s2.Remark) diffs.Add("备注变更");
        return diffs.Count > 0 ? string.Join("；", diffs) : "无差异";
    }

    private static string BumpVersion(string version)
    {
        if (version.StartsWith("V", StringComparison.OrdinalIgnoreCase) &&
            decimal.TryParse(version[1..], out var ver))
        {
            return $"V{(ver + 1):F1}";
        }
        return "V1.0";
    }
}

