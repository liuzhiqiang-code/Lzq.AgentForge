using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Application.Contracts.ReferenceData;
using Lzq.MES.Domain.IRepositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.MES.Application.Services.ReferenceData;

/// <summary>
/// 基础数据引用服务实现
/// 通过 BaseData 模块的仓储获取 Line、Process 等基础数据
/// 遵循模块解耦原则：实现类在 Application 层，接口在 Application.Contracts 层
/// </summary>
public class ReferenceDataService : ServiceBase, IReferenceDataService
{
    private ILineRepository LineRepo => GetRequiredService<ILineRepository>();
    private IProcessRepository ProcessRepo => GetRequiredService<IProcessRepository>();

    [IgnoreRoute]
    public async Task<LineSimpleDto?> GetLineByIdAsync(long lineId)
    {
        var entity = await LineRepo.GetByIdAsync(lineId);
        if (entity == null) return null;

        return new LineSimpleDto
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
    [IgnoreRoute]
    public async Task<List<LineSimpleDto>> GetLinesByIdsAsync(List<long> lineIds)
    {
        if (lineIds == null || !lineIds.Any())
            return new List<LineSimpleDto>();

        var entities = await LineRepo.AsQueryable()
            .Where(l => lineIds.Contains(l.Id))
            .ToListAsync();

        return entities.Select(e => new LineSimpleDto
        {
            Id = e.Id,
            Name = e.Name
        }).ToList();
    }
    [IgnoreRoute]
    public async Task<ProcessSimpleDto?> GetProcessByIdAsync(long processId)
    {
        var entity = await ProcessRepo.GetByIdAsync(processId);
        if (entity == null) return null;

        return new ProcessSimpleDto
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
    [IgnoreRoute]
    public async Task<List<ProcessSimpleDto>> GetProcessesByIdsAsync(List<long> processIds)
    {
        if (processIds == null || !processIds.Any())
            return new List<ProcessSimpleDto>();

        var entities = await ProcessRepo.AsQueryable()
            .Where(p => processIds.Contains(p.Id))
            .ToListAsync();

        return entities.Select(e => new ProcessSimpleDto
        {
            Id = e.Id,
            Name = e.Name
        }).ToList();
    }
    [IgnoreRoute]
    public async Task<bool> LineExistsAsync(long lineId)
    {
        return await LineRepo.GetByIdAsync(lineId) != null;
    }
    [IgnoreRoute]
    public async Task<bool> ProcessExistsAsync(long processId)
    {
        return await ProcessRepo.GetByIdAsync(processId) != null;
    }
}
