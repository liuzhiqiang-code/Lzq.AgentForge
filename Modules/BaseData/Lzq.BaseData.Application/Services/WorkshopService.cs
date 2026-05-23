using Microsoft.AspNetCore.Builder;
using Lzq.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Lzq.BaseData.Application.Contracts.Commands;
using Lzq.BaseData.Application.Contracts.Queries;
using Lzq.BaseData.Application.Contracts.IServices;
using Lzq.BaseData.Domain.Entities;
using Lzq.BaseData.Domain.Enums;
using Lzq.BaseData.Domain.IRepositories;
using NSwag.Annotations;
using SqlSugar;
using Lzq.BaseData.Application.Contracts.Dtos;
using Mapster;

namespace Lzq.BaseData.Application.Services;

/// <summary>
/// 车间管理服务
/// </summary>
public class WorkshopService : ServiceBase, IWorkshopService
{
    public WorkshopService() : base("/api/v1/mes/workshop") { }

    private IWorkshopRepository WorkshopRepo => GetRequiredService<IWorkshopRepository>();
    private IFactoryRepository FactoryRepo => GetRequiredService<IFactoryRepository>();
    private ILineRepository LineRepo => GetRequiredService<ILineRepository>();
    private IProcessRepository ProcessRepo => GetRequiredService<IProcessRepository>();

    // ========== 分页查询 ==========
    [OpenApiTag("mes/workshop"), OpenApiOperation("分页查询车间", "支持按工厂、编码、名称筛选，自动填充工厂名称")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<WorkshopDto>>> PageAsync([FromBody] WorkshopPageQuery query)
    {
        var expr = WorkshopRepo.AsQueryable()
            .WhereIF(query.FactoryId.HasValue, w => w.FactoryId == query.FactoryId!.Value)
            .WhereIF(query.Status.HasValue, w => w.Status == query.Status!.Value)
            .WhereIF(!string.IsNullOrEmpty(query.Code), w => w.Code!.Contains(query.Code!))
            .WhereIF(!string.IsNullOrEmpty(query.Name), w => w.Name!.Contains(query.Name!));

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(w => w.CreationTime, OrderByType.Desc)
            .ToPageListAsync(query.Page, query.PageSize, total);

        var dtos = list.Map<List<WorkshopDto>>();
        await FillFactoryNames(dtos);

        return ApiResult.Success(new PagedResponse<WorkshopDto>(dtos, total));
    }

    // ========== 按工厂获取（前端下拉框用）==========
    [OpenApiTag("mes/workshop"), OpenApiOperation("按工厂获取车间列表", "用于前端联动选择器，返回指定工厂下所有启用车间")]
    [RoutePattern(pattern: "list-by-factory/{factoryId}", true)]
    public async Task<ApiResult<List<WorkshopDto>>> ListByFactoryAsync(long factoryId)
    {
        var workshops = await WorkshopRepo.AsQueryable()
            .Where(w => w.FactoryId == factoryId && w.Status == EnableStatusEnum.Enabled)
            .OrderBy(w => w.Code).ToListAsync();
        return ApiResult.Success(workshops.Map<List<WorkshopDto>>());
    }

    // ========== 车间→产线树 ==========
    [OpenApiTag("mes/workshop"), OpenApiOperation("获取车间产线树", "车间→产线→工序树形结构，按工厂ID过滤")]
    [RoutePattern(pattern: "tree/{factoryId}", true)]
    public async Task<ApiResult<List<WorkshopTreeDto>>> TreeAsync(long factoryId)
    {
        var workshops = await WorkshopRepo.AsQueryable()
            .Where(w => w.FactoryId == factoryId && w.Status == EnableStatusEnum.Enabled)
            .OrderBy(w => w.Code).ToListAsync();
        if (!workshops.Any()) return ApiResult.Success(new List<WorkshopTreeDto>());

        var workshopIds = workshops.Select(w => w.Id).ToList();
        var lines = await LineRepo.AsQueryable()
            .Where(l => workshopIds.Contains(l.WorkshopId) && l.Status == EnableStatusEnum.Enabled)
            .OrderBy(l => l.Code).ToListAsync();
        var lineIds = lines.Select(l => l.Id).ToList();

        var processes = lineIds.Any()
            ? await ProcessRepo.AsQueryable().Where(p => lineIds.Contains(p.LineId) && p.Status == EnableStatusEnum.Enabled).OrderBy(p => p.Sequence).ToListAsync()
            : [];

        var tree = workshops.Select(w => new WorkshopTreeDto
        {
            Id = w.Id, Code = w.Code, Name = w.Name, FactoryId = w.FactoryId, Manager = w.Manager,
            Children = lines.Where(l => l.WorkshopId == w.Id).Select(l => new LineTreeDto
            {
                Id = l.Id, Code = l.Code, Name = l.Name, WorkshopId = l.WorkshopId,
                Children = processes.Where(p => p.LineId == l.Id).Select(p => new ProcessTreeDto
                {
                    Id = p.Id, Code = p.Code, Name = p.Name, Sequence = p.Sequence, StandardHours = p.StandardHours,
                }).ToList(),
            }).ToList(),
        }).ToList();

        return ApiResult.Success(tree);
    }

    // ========== CRUD ==========
    [OpenApiTag("mes/workshop"), OpenApiOperation("创建车间", "新建车间，关联工厂")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] WorkshopCreateCommand command)
    {
        var factoryExists = await FactoryRepo.AsQueryable().AnyAsync(f => f.Id == command.FactoryId);
        if (!factoryExists) throw new UserFriendlyException("所属工厂不存在");

        var codeExists = await WorkshopRepo.AsQueryable().AnyAsync(w => w.Code == command.Code);
        if (codeExists) throw new UserFriendlyException($"车间编码 [{command.Code}] 已存在");

        var entity = command.Map<WorkshopEntity>();
        await WorkshopRepo.InsertAsync(entity);
        return ApiResult.Success(entity.Id);
    }

    [OpenApiTag("mes/workshop"), OpenApiOperation("更新车间", "根据ID更新车间信息")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult<bool>> UpdateAsync([FromBody] WorkshopUpdateCommand command)
    {
        var entity = await WorkshopRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("车间不存在");
        var codeExists = await WorkshopRepo.AsQueryable()
            .AnyAsync(w => w.Code == command.Code && w.Id != command.Id);
        if (codeExists) throw new UserFriendlyException($"车间编码 [{command.Code}] 已被其他车间使用");

        command.Adapt(entity);
        await WorkshopRepo.UpdateAsync(entity);
        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/workshop"), OpenApiOperation("删除车间", "级联删除车间")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await WorkshopRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("车间不存在");
        await WorkshopRepo.DeleteAsync(entity);
        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/workshop"), OpenApiOperation("批量删除车间", "批量级联删除车间")]
    [RoutePattern(pattern: "batch-delete", true)]
    public async Task<ApiResult<int>> BatchDeleteAsync([FromBody] List<long> ids)
    {
        if (ids is null || ids.Count == 0) throw new UserFriendlyException("请选择要删除的车间");
        await WorkshopRepo.DeleteAsync(w => ids.Contains(w.Id));
        return ApiResult.Success(ids.Count);
    }

    // ========== Helper ==========
    private async Task FillFactoryNames(List<WorkshopDto> dtos)
    {
        if (!dtos.Any()) return;
        var factoryIds = dtos.Select(d => d.FactoryId).Distinct().ToList();
        var factories = await FactoryRepo.AsQueryable().Where(f => factoryIds.Contains(f.Id)).ToListAsync();
        for (int i = 0; i < dtos.Count; i++)
        {
            dtos[i] = dtos[i] with { FactoryName = factories.FirstOrDefault(f => f.Id == dtos[i].FactoryId)?.Name };
        }
    }
}
