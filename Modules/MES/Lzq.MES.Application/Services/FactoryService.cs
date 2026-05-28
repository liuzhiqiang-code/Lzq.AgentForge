using Microsoft.AspNetCore.Builder;
using Lzq.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.Enums;
using Lzq.MES.Domain.IRepositories;
using Lzq.MES.Domain.Consts;
using Lzq.Extensions.Redis;
using NSwag.Annotations;
using SqlSugar;
using Lzq.MES.Application.Contracts.Dtos;

namespace Lzq.MES.Application.Services;

/// <summary>
/// 工厂管理服务
/// </summary>
public class FactoryService : ServiceBase, IFactoryService
{
    public FactoryService() : base("/api/v1/mes/factory") { }

    private IFactoryRepository FactoryRepo => GetRequiredService<IFactoryRepository>();
    private IWorkshopRepository WorkshopRepo => GetRequiredService<IWorkshopRepository>();
    private ILineRepository LineRepo => GetRequiredService<ILineRepository>();
    private IProcessRepository ProcessRepo => GetRequiredService<IProcessRepository>();

    // ========== 分页查询 ==========
    [OpenApiTag("mes/factory"), OpenApiOperation("分页查询工厂", "支持按编码、名称、状态筛选")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<FactoryDto>>> PageAsync([FromBody] FactoryPageQuery query)
    {
        var expr = FactoryRepo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Code), f => f.Code!.Contains(query.Code!))
            .WhereIF(!string.IsNullOrEmpty(query.Name), f => f.Name!.Contains(query.Name!))
            .WhereIF(query.Status.HasValue, f => f.Status == (EnableStatusEnum)query.Status!.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(f => f.CreationTime, OrderByType.Desc)
            .ToPageListAsync(query.Page, query.PageSize, total);

        return ApiResult.Success(new PagedResponse<FactoryDto>(list.Map<List<FactoryDto>>(), total));
    }

    // ========== 完整工厂树（Factory→Workshop→Line→Process）==========
    [OpenApiTag("mes/factory"), OpenApiOperation("获取完整工厂树", "四级树形结构：工厂→车间→产线→工序，用于产线选择器和看板展示")]
    [RoutePattern(pattern: "tree", true)]
    public async Task<ApiResult<List<FactoryTreeDto>>> TreeAsync()
    {
        var redis = GetRequiredService<ILzqRedisClient>();
        var cacheKey = RedisKeys.FactoryTree;
        
        var tree = await redis.GetOrSetAsync(cacheKey, async () =>
        {
            var factories = await FactoryRepo.AsQueryable()
                .Where(f => f.Status == EnableStatusEnum.Enabled)
                .OrderBy(f => f.Code).ToListAsync();
            if (!factories.Any()) return new List<FactoryTreeDto>();

            var factoryIds = factories.Select(f => f.Id).ToList();
            var workshops = await WorkshopRepo.AsQueryable()
                .Where(w => factoryIds.Contains(w.FactoryId) && w.Status == EnableStatusEnum.Enabled)
                .OrderBy(w => w.Code).ToListAsync();
            var workshopIds = workshops.Select(w => w.Id).ToList();

            var lines = workshopIds.Any()
                ? await LineRepo.AsQueryable().Where(l => workshopIds.Contains(l.WorkshopId) && l.Status == EnableStatusEnum.Enabled).OrderBy(l => l.Code).ToListAsync()
                : [];
            var lineIds = lines.Select(l => l.Id).ToList();

            var processes = lineIds.Any()
                ? await ProcessRepo.AsQueryable().Where(p => lineIds.Contains(p.LineId) && p.Status == EnableStatusEnum.Enabled).OrderBy(p => p.Sequence).ToListAsync()
                : [];

            var result = factories.Select(f => new FactoryTreeDto
            {
                Id = f.Id,
                Code = f.Code,
                Name = f.Name,
                Children = workshops.Where(w => w.FactoryId == f.Id).Select(w => new WorkshopTreeDto
                {
                    Id = w.Id,
                    Code = w.Code,
                    Name = w.Name,
                    FactoryId = w.FactoryId,
                    Manager = w.Manager,
                    Children = lines.Where(l => l.WorkshopId == w.Id).Select(l => new LineTreeDto
                    {
                        Id = l.Id,
                        Code = l.Code,
                        Name = l.Name,
                        WorkshopId = l.WorkshopId,
                        Children = processes.Where(p => p.LineId == l.Id).Select(p => new ProcessTreeDto
                        {
                            Id = p.Id,
                            Code = p.Code,
                            Name = p.Name,
                            Sequence = p.Sequence,
                            StandardHours = p.StandardHours,
                        }).ToList(),
                    }).ToList(),
                }).ToList(),
            }).ToList();

            return result;
        }, TimeSpan.FromHours(2)); // 缓存2小时，工厂树变更不频繁

        return ApiResult.Success(tree ?? new List<FactoryTreeDto>());
    }

    // ========== CRUD ==========
    [OpenApiTag("mes/factory"), OpenApiOperation("创建工厂", "新建工厂记录，编码不可重复")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] FactoryCreateCommand command)
    {
        var exists = await FactoryRepo.AsQueryable().AnyAsync(f => f.Code == command.Code);
        if (exists) throw new UserFriendlyException($"工厂编码 [{command.Code}] 已存在");

        var entity = command.Map<FactoryEntity>();
        await FactoryRepo.InsertAsync(entity);
        
        // 清除工厂树缓存，保证数据一致性
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.FactoryTree);
        
        return ApiResult.Success(entity.Id);
    }

    [OpenApiTag("mes/factory"), OpenApiOperation("更新工厂", "根据ID更新工厂信息")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult<bool>> UpdateAsync([FromBody] FactoryUpdateCommand command)
    {
        var entity = await FactoryRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("工厂不存在");

        var codeExists = await FactoryRepo.AsQueryable()
            .AnyAsync(f => f.Code == command.Code && f.Id != command.Id);
        if (codeExists) throw new UserFriendlyException($"工厂编码 [{command.Code}] 已被其他工厂使用");

        command.Map(entity);
        await FactoryRepo.UpdateAsync(entity);
        
        // 清除工厂树缓存，保证数据一致性
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.FactoryTree);
        
        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/factory"), OpenApiOperation("删除工厂", "级联删除工厂记录")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await FactoryRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("工厂不存在");
        await FactoryRepo.DeleteAsync(entity);
        
        // 清除工厂树缓存，保证数据一致性
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.FactoryTree);
        
        return ApiResult.Success(true);
    }

    [OpenApiTag("mes/factory"), OpenApiOperation("批量删除工厂", "批量级联删除工厂")]
    [RoutePattern(pattern: "batch-delete", true)]
    public async Task<ApiResult<int>> BatchDeleteAsync([FromBody] List<long> ids)
    {
        if (ids is null || ids.Count == 0) throw new UserFriendlyException("请选择要删除的工厂");
        await FactoryRepo.DeleteAsync(f => ids.Contains(f.Id));
        
        // 清除工厂树缓存，保证数据一致性
        var redis = GetRequiredService<ILzqRedisClient>();
        await redis.RemoveAsync(RedisKeys.FactoryTree);
        
        return ApiResult.Success(ids.Count);
    }
}
