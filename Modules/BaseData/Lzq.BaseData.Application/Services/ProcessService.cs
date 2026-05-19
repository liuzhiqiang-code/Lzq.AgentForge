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

namespace Lzq.BaseData.Application.Services;

/// <summary>
/// 工序管理服务
/// </summary>
public class ProcessService : ServiceBase, IProcessService
{
    public ProcessService() : base("/api/v1/mes/process") { }

    private IProcessRepository ProcessRepo => GetRequiredService<IProcessRepository>();
    private ILineRepository LineRepo => GetRequiredService<ILineRepository>();

    [OpenApiTag("mes/process"), OpenApiOperation("分页查询工序", "支持按产线、编码、名称筛选，按工序顺序排序，自动填充产线名称")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<ProcessDto>>> PageAsync([FromBody] ProcessPageQuery query)
    {
        var expr = ProcessRepo.AsQueryable()
            .WhereIF(query.LineId.HasValue, p => p.LineId == query.LineId!.Value)
            .WhereIF(!string.IsNullOrEmpty(query.Code), p => p.Code!.Contains(query.Code!))
            .WhereIF(query.Status.HasValue, p => p.Status == query.Status!.Value)
            .WhereIF(!string.IsNullOrEmpty(query.Name), p => p.Name!.Contains(query.Name!));

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(p => p.Sequence)
            .ToPageListAsync(query.Page, query.PageSize, total);

        var dtos = list.Map<List<ProcessDto>>();
        await FillLineNames(dtos);

        return ApiResult.Success(new PagedResponse<ProcessDto>(dtos, total));
    }

    [OpenApiTag("mes/process"), OpenApiOperation("按产线获取工序列表", "用于前端联动选择器，返回指定产线所有启用工序，按顺序排序")]
    [RoutePattern(pattern: "list-by-line/{lineId}", true)]
    public async Task<ApiResult<List<ProcessDto>>> ListByLineAsync(long lineId)
    {
        var processes = await ProcessRepo.AsQueryable()
            .Where(p => p.LineId == lineId && p.Status == EnableStatusEnum.Enabled)
            .OrderBy(p => p.Sequence).ToListAsync();
        return ApiResult.Success(processes.Map<List<ProcessDto>>());
    }

    [OpenApiTag("mes/process"), OpenApiOperation("创建工序", "新建工序，关联产线，指定顺序和标准工时")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] ProcessCreateCommand command)
    {
        var lineExists = await LineRepo.AsQueryable().AnyAsync(l => l.Id == command.LineId);
        if (!lineExists) throw new UserFriendlyException("所属产线不存在");

        var codeExists = await ProcessRepo.AsQueryable().AnyAsync(p => p.Code == command.Code);
        if (codeExists) throw new UserFriendlyException($"工序编码 [{command.Code}] 已存在");

        var entity = command.Map<ProcessEntity>();
        await ProcessRepo.InsertAsync(entity);
        return ApiResult.Success(entity.Id);
    }

    [OpenApiTag("mes/process"), OpenApiOperation("更新工序", "根据ID更新工序信息")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult<bool>> UpdateAsync([FromBody] ProcessUpdateCommand command)
    {
        var entity = await ProcessRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("工序不存在");
        var codeExists = await ProcessRepo.AsQueryable()
            .AnyAsync(p => p.Code == command.Code && p.Id != command.Id);
        if (codeExists) throw new UserFriendlyException($"工序编码 [{command.Code}] 已被其他工序使用");

        command.Map(entity);
        await ProcessRepo.UpdateAsync(entity);
        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/process"), OpenApiOperation("删除工序", "级联删除工序")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await ProcessRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("工序不存在");
        await ProcessRepo.DeleteAsync(entity);
        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/process"), OpenApiOperation("批量删除工序", "批量级联删除工序")]
    [RoutePattern(pattern: "batch-delete", true)]
    public async Task<ApiResult<int>> BatchDeleteAsync([FromBody] List<long> ids)
    {
        if (ids is null || ids.Count == 0) throw new UserFriendlyException("请选择要删除的工序");
        await ProcessRepo.DeleteAsync(p => ids.Contains(p.Id));
        return ApiResult.Success(ids.Count);
    }

    private async Task FillLineNames(List<ProcessDto> dtos)
    {
        if (!dtos.Any()) return;
        var lineIds = dtos.Select(d => d.LineId).Distinct().ToList();
        var lines = await LineRepo.AsQueryable().Where(l => lineIds.Contains(l.Id)).ToListAsync();
        for (int i = 0; i < dtos.Count; i++)
        {
            dtos[i] = dtos[i] with { LineName = lines.FirstOrDefault(l => l.Id == dtos[i].LineId)?.Name };
        }
    }
}
