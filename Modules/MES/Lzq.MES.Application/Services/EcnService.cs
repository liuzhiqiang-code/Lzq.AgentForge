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

public class EcnService : ServiceBase, IEcnService
{
    public EcnService() : base("/api/v1/mes/ecn") { }

    private IEcnRepository Repo => GetRequiredService<IEcnRepository>();
    private IEcnItemRepository ItemRepo => GetRequiredService<IEcnItemRepository>();
    private IBomRepository BomRepo => GetRequiredService<IBomRepository>();
    private IBomItemRepository BomItemRepo => GetRequiredService<IBomItemRepository>();
    private IMaterialRepository MaterialRepo => GetRequiredService<IMaterialRepository>();
    private IBomVersionHistoryRepository HistoryRepo => GetRequiredService<IBomVersionHistoryRepository>();

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [OpenApiTag("mes/ecn"), OpenApiOperation("分页查询 ECN", "支持按编码、标题、状态筛閫?")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<EcnDto>>> PageAsync([FromBody] EcnPageQuery query)
    {
        var expr = Repo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Code), e => e.Code!.Contains(query.Code!))
            .WhereIF(!string.IsNullOrEmpty(query.Title), e => e.Title!.Contains(query.Title!))
            .WhereIF(query.Status.HasValue, e => e.Status == query.Status!.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderByDescending(e => e.CreationTime)
            .ToPageListAsync(query.Page, query.PageSize, total);

        return ApiResult.Success(new PagedResponse<EcnDto>(list.Map<List<EcnDto>>(), total));
    }

    [OpenApiTag("mes/ecn"), OpenApiOperation("获取 ECN 详情", "包含变更项列表")]
    [RoutePattern(pattern: "get/{id}", true)]
    public async Task<ApiResult<EcnDetailDto>> GetAsync(long id)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("ECN 涓嶅瓨鍦?");

        var header = entity.Map<EcnDto>();
        var items = await ItemRepo.AsQueryable()
            .Where(i => i.EcnId == id)
            .OrderBy(i => i.Id)
            .ToListAsync();

        return ApiResult.Success(new EcnDetailDto
        {
            Header = header,
            Items = items.Map<List<EcnItemDto>>(),
        });
    }

    [OpenApiTag("mes/ecn"), OpenApiOperation("鍒涘缓 ECN")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] EcnCreateCommand command)
    {
        var exists = await Repo.AsQueryable().AnyAsync(e => e.Code == command.Code);
        if (exists) throw new UserFriendlyException($"ECN 缂栫爜 [{command.Code}] 宸插瓨鍦?");

        var entity = command.Map<EcnEntity>();
        entity.Status = EcnStatusEnum.Draft;
        await Repo.InsertAsync(entity);

        return ApiResult.Success(entity.Id);
    }

    [OpenApiTag("mes/ecn"), OpenApiOperation("更新 ECN", "仅草稿状态可修改")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult<bool>> UpdateAsync([FromBody] EcnUpdateCommand command)
    {
        var entity = await Repo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("ECN 涓嶅瓨鍦?");

        if (entity.Status != EcnStatusEnum.Draft)
            throw new UserFriendlyException("只有草稿状态的 ECN 允许修改");

        var codeExists = await Repo.AsQueryable().AnyAsync(e => e.Code == command.Code && e.Id != command.Id);
        if (codeExists) throw new UserFriendlyException($"ECN 缂栫爜 [{command.Code}] 已被使用");

        command.Map(entity);
        await Repo.UpdateAsync(entity);

        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/ecn"), OpenApiOperation("鍒犻櫎 ECN", "浠呰崏绋垮彲鍒犻櫎")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("ECN 涓嶅瓨鍦?");

        if (entity.Status != EcnStatusEnum.Draft)
            throw new UserFriendlyException("只有草稿状态的 ECN 允许鍒犻櫎");

        await ItemRepo.DeleteAsync(i => i.EcnId == id);
        await Repo.DeleteAsync(entity);

        return ApiResult.Success(true);
    }

    // ==================== ECN 鍙樻洿椤?====================

    [OpenApiTag("mes/ecn"), OpenApiOperation("获取 ECN 鍙樻洿椤瑰垪琛?")]
    [RoutePattern(pattern: "items/{ecnId}", true)]
    public async Task<ApiResult<List<EcnItemDto>>> GetItemsAsync(long ecnId)
    {
        var ecnExists = await Repo.AsQueryable().AnyAsync(e => e.Id == ecnId);
        if (!ecnExists) throw new UserFriendlyException("ECN 涓嶅瓨鍦?");

        var items = await ItemRepo.AsQueryable()
            .Where(i => i.EcnId == ecnId)
            .OrderBy(i => i.Id)
            .ToListAsync();

        return ApiResult.Success(items.Map<List<EcnItemDto>>());
    }

    [OpenApiTag("mes/ecn"), OpenApiOperation("娣诲姞鍙樻洿椤?")]
    [RoutePattern(pattern: "item/create", true)]
    public async Task<ApiResult<long>> CreateItemAsync([FromBody] EcnItemCreateCommand command)
    {
        var ecn = await Repo.GetByIdAsync(command.EcnId)
            ?? throw new UserFriendlyException("ECN 涓嶅瓨鍦?");

        if (ecn.Status != EcnStatusEnum.Draft)
            throw new UserFriendlyException("只有草稿状态的 ECN 允许娣诲姞鍙樻洿椤?");

        var entity = command.Map<EcnItemEntity>();
        await ItemRepo.InsertAsync(entity);

        return ApiResult.Success(entity.Id);
    }

    [OpenApiTag("mes/ecn"), OpenApiOperation("鍒犻櫎鍙樻洿椤?")]
    [RoutePattern(pattern: "item/delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteItemAsync(long id)
    {
        var entity = await ItemRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("鍙樻洿椤逛笉瀛樺湪");

        var ecn = await Repo.GetByIdAsync(entity.EcnId);
        if (ecn?.Status != EcnStatusEnum.Draft)
            throw new UserFriendlyException("只有草稿状态的 ECN 允许鍒犻櫎鍙樻洿椤?");

        await ItemRepo.DeleteAsync(entity);

        return ApiResult.Success(true);
    }

    // ==================== ECN 工作流===================

    [OpenApiTag("mes/ecn"), OpenApiOperation("提交审批", "草稿→待审批")]
    [RoutePattern(pattern: "submit/{id}", true)]
    public async Task<ApiResult<bool>> SubmitAsync(long id)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("ECN 涓嶅瓨鍦?");

        if (entity.Status != EcnStatusEnum.Draft)
            throw new UserFriendlyException("只有草稿状态的 ECN 可以提交");

        var hasItems = await ItemRepo.AsQueryable().AnyAsync(i => i.EcnId == id);
        if (!hasItems) throw new UserFriendlyException("请至少添加一条变更项后再提交");

        entity.Status = EcnStatusEnum.Pending;
        await Repo.UpdateAsync(entity);

        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/ecn"), OpenApiOperation("审批通过", "待审批→已批准")]
    [RoutePattern(pattern: "approve/{id}", true)]
    public async Task<ApiResult<bool>> ApproveAsync(long id)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("ECN 涓嶅瓨鍦?");

        if (entity.Status != EcnStatusEnum.Pending)
            throw new UserFriendlyException("只有待审批状态的 ECN 可以审批");

        entity.Status = EcnStatusEnum.Approved;
        entity.ApprovedAt = DateTime.Now;
        await Repo.UpdateAsync(entity);

        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/ecn"), OpenApiOperation("执行变更", "宸叉壒鍑嗏啋宸叉墽琛岋紝更新瀹為檯鏁版嵁")]
    [RoutePattern(pattern: "execute", true)]
    public async Task<ApiResult<bool>> ExecuteAsync([FromBody] EcnExecuteCommand command)
    {
        var entity = await Repo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("ECN 涓嶅瓨鍦?");

        if (entity.Status != EcnStatusEnum.Approved)
            throw new UserFriendlyException("只有已批准状态的 ECN 可以执行");

        foreach (var item in command.Items)
        {
            var changeItem = await ItemRepo.GetByIdAsync(item.EcnItemId)
                ?? throw new UserFriendlyException($"鍙樻洿椤?{item.EcnItemId} 涓嶅瓨鍦?");

            changeItem.AfterSnapshot = item.AfterSnapshot;
            await ItemRepo.UpdateAsync(changeItem);
        }

        entity.Status = EcnStatusEnum.Executed;
        entity.ExecutedAt = DateTime.Now;
        await Repo.UpdateAsync(entity);

        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/ecn"), OpenApiOperation("确认变更", "已执行→已确认")]
    [RoutePattern(pattern: "confirm/{id}", true)]
    public async Task<ApiResult<bool>> ConfirmAsync(long id)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("ECN 涓嶅瓨鍦?");

        if (entity.Status != EcnStatusEnum.Executed)
            throw new UserFriendlyException("只有已执行状态的 ECN 可以确认");

        entity.Status = EcnStatusEnum.Confirmed;
        entity.ConfirmedAt = DateTime.Now;
        await Repo.UpdateAsync(entity);

        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/ecn"), OpenApiOperation("鍙栨秷 ECN", "任意状态→已取消")]
    [RoutePattern(pattern: "cancel/{id}", true)]
    public async Task<ApiResult<bool>> CancelAsync(long id)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("ECN 涓嶅瓨鍦?");

        if (entity.Status is EcnStatusEnum.Confirmed or EcnStatusEnum.Cancelled)
            throw new UserFriendlyException("已确认或已取消的 ECN 无法取消");

        entity.Status = EcnStatusEnum.Cancelled;
        await Repo.UpdateAsync(entity);

        return ApiResult.Success(true);
    }

    // ==================== 褰卞搷鍒嗘瀽 ====================

    [OpenApiTag("mes/ecn"), OpenApiOperation("褰卞搷鍒嗘瀽", "鍒嗘瀽鍙樻洿娑夊強鐨勬墍鏈?BOM/物料范围")]
    [RoutePattern(pattern: "analyze-impact/{id}", true)]
    public async Task<ApiResult<string>> AnalyzeImpactAsync(long id)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("ECN 涓嶅瓨鍦?");

        if (entity.Status == EcnStatusEnum.Draft)
            throw new UserFriendlyException("璇峰厛鎻愪氦后再进行影响分析");

        var items = await ItemRepo.AsQueryable()
            .Where(i => i.EcnId == id)
            .ToListAsync();

        var affectedProductIds = new HashSet<long>();
        var affectedBomIds = new HashSet<long>();
        var affectedMaterialIds = new HashSet<long>();

        foreach (var item in items)
        {
            switch (item.ChangeType)
            {
                case EcnChangeTypeEnum.Material:
                    affectedMaterialIds.Add(item.TargetId);
                    var matBomItems = await BomItemRepo.AsQueryable()
                        .Where(bi => bi.ItemId == item.TargetId)
                        .ToListAsync();
                    foreach (var bi in matBomItems)
                    {
                        affectedBomIds.Add(bi.BomId);
                    }
                    break;

                case EcnChangeTypeEnum.Bom:
                    affectedBomIds.Add(item.TargetId);
                    break;
            }
        }

        // 浠庡彈褰卞搷鐨?BOM 反查产品
        if (affectedBomIds.Count > 0)
        {
            var boms = await BomRepo.AsQueryable()
                .Where(b => affectedBomIds.Contains(b.Id))
                .ToListAsync();
            foreach (var bom in boms)
            {
                affectedProductIds.Add(bom.ProductId);
            }
        }

        var impact = new
        {
            materials = affectedMaterialIds.Count,
            materialIds = affectedMaterialIds.ToList(),
            boms = affectedBomIds.Count,
            bomIds = affectedBomIds.ToList(),
            products = affectedProductIds.Count,
            productIds = affectedProductIds.ToList(),
            summary = $"变更将影响 {affectedMaterialIds.Count} 个物料、{affectedBomIds.Count} 个 BOM、{affectedProductIds.Count} 个产品",
        };

        var impactJson = JsonSerializer.Serialize(impact, _jsonOpts);

        entity.ImpactAnalysis = impactJson;
        await Repo.UpdateAsync(entity);

        return ApiResult.Success<string>(impactJson);
    }
}

