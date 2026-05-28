using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.Enums;
using Lzq.MES.Domain.IRepositories;
using Microsoft.AspNetCore.Builder;
using Lzq.Core.Models;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SqlSugar;
using System.Collections;
using Lzq.MES.Application.Contracts.Dtos;

namespace Lzq.MES.Application.Services;

/// <summary>
/// 产线管理服务
/// </summary>
public class LineService : ServiceBase, ILineService
{
    public LineService() : base("/api/v1/mes/line") { }

    private ILineRepository LineRepo => GetRequiredService<ILineRepository>();
    private IWorkshopRepository WorkshopRepo => GetRequiredService<IWorkshopRepository>();

    [OpenApiTag("mes/line"), OpenApiOperation("分页查询产线", "支持按车间、编码、名称筛选，自动填充车间名称")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<LineDto>>> PageAsync([FromBody] LinePageQuery query)
    {
        var expr = LineRepo.AsQueryable()
            .WhereIF(query.WorkshopId.HasValue, l => l.WorkshopId == query.WorkshopId!.Value)
            .WhereIF(!string.IsNullOrEmpty(query.Code), l => l.Code!.Contains(query.Code!))
            .WhereIF(query.Status.HasValue, l => l.Status == query.Status!.Value)
            .WhereIF(!string.IsNullOrEmpty(query.Name), l => l.Name!.Contains(query.Name!));

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(l => l.CreationTime, OrderByType.Desc)
            .ToPageListAsync(query.Page, query.PageSize, total);

        var dtos = list.Map<List<LineDto>>();
        await FillWorkshopNames(dtos);

        return ApiResult.Success(new PagedResponse<LineDto>(dtos, total));
    }

    [OpenApiTag("mes/line"), OpenApiOperation("按车间获取产线列表", "用于前端联动选择器，返回指定车间下所有启用产线")]
    [RoutePattern(pattern: "list-by-workshop/{workshopId}", true)]
    public async Task<ApiResult<List<LineDto>>> ListByWorkshopAsync(long workshopId)
    {
        var lines = await LineRepo.AsQueryable()
            .Where(l => l.WorkshopId == workshopId && l.Status == EnableStatusEnum.Enabled)
            .OrderBy(l => l.Code).ToListAsync();
        return ApiResult.Success(lines.Map<List<LineDto>>());
    }

    [OpenApiTag("mes/line"), OpenApiOperation("创建产线", "新建产线，关联车间")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] LineCreateCommand command)
    {
        var workshopExists = await WorkshopRepo.AsQueryable().AnyAsync(w => w.Id == command.WorkshopId);
        if (!workshopExists) throw new UserFriendlyException("所属车间不存在");

        var codeExists = await LineRepo.AsQueryable().AnyAsync(l => l.Code == command.Code);
        if (codeExists) throw new UserFriendlyException($"产线编码 [{command.Code}] 已存在");

        var entity = command.Map<LineEntity>();
        await LineRepo.InsertAsync(entity);
        return ApiResult.Success(entity.Id);
    }

    [OpenApiTag("mes/line"), OpenApiOperation("更新产线", "根据ID更新产线信息")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult<bool>> UpdateAsync([FromBody] LineUpdateCommand command)
    {
        var entity = await LineRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("产线不存在");
        var codeExists = await LineRepo.AsQueryable()
            .AnyAsync(l => l.Code == command.Code && l.Id != command.Id);
        if (codeExists) throw new UserFriendlyException($"产线编码 [{command.Code}] 已被其他产线使用");

        command.Map(entity);
        await LineRepo.UpdateAsync(entity);
        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/line"), OpenApiOperation("删除产线", "级联删除产线")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await LineRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("产线不存在");
        await LineRepo.DeleteAsync(entity);
        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/line"), OpenApiOperation("批量删除产线", "批量级联删除产线")]
    [RoutePattern(pattern: "batch-delete", true)]
    public async Task<ApiResult<int>> BatchDeleteAsync([FromBody] List<long> ids)
    {
        if (ids is null || ids.Count == 0) throw new UserFriendlyException("请选择要删除的产线");
        await LineRepo.DeleteAsync(l => ids.Contains(l.Id));
        return ApiResult.Success(ids.Count);
    }

    private async Task FillWorkshopNames(List<LineDto> dtos)
    {
        if (!dtos.Any()) return;
        var workshopIds = dtos.Select(d => d.WorkshopId).Distinct().ToList();
        var workshops = await WorkshopRepo.AsQueryable().Where(w => workshopIds.Contains(w.Id)).ToListAsync();
        for (int i = 0; i < dtos.Count; i++)
        {
            dtos[i] = dtos[i] with { WorkshopName = workshops.FirstOrDefault(w => w.Id == dtos[i].WorkshopId)?.Name };
        }
    }

    public async Task<LineDto?> GetByIdAsync(long id)
    {
        var line = await LineRepo.AsQueryable()
             .Where(l => l.Id == id)
             .FirstAsync();
        if (line == null) return null;
        return line.Map<LineDto>();
    }
}
