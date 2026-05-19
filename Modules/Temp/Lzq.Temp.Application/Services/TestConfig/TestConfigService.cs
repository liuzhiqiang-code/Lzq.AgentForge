using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Lzq.Core.Models;
using Lzq.Temp.Application.Contracts.IServices;
using Lzq.Temp.Application.Contracts.TestConfig.Commands;
using Lzq.Temp.Application.Contracts.TestConfig.Dto;
using Lzq.Temp.Application.Contracts.TestConfig.Queries;
using Lzq.Temp.Domain.Entities.TestConfig;
using Lzq.Extensions.SqlSugar.Repository;
using Mapster;
using NSwag.Annotations;
using SqlSugar;

namespace Lzq.Temp.Application.Services.TestConfig;

/// <summary>
/// 测试配置服务实现
/// </summary>
public class TestConfigService : ServiceBase, ITestConfigService
{
    public TestConfigService() : base("/api/v1/temp/config") { }

    private ISqlSugarRepository<TestConfigEntity> ConfigRepo => GetRequiredService<ISqlSugarRepository<TestConfigEntity>>();

    [OpenApiTag("temp/config")]
    [OpenApiOperation("分页查询测试配置", "支持按关键字、配置类型筛选")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<TestConfigViewDto>>> PageAsync(
        [FromBody] TestConfigPageQuery query)
    {
        var expr = ConfigRepo.AsQueryable();

        if (!string.IsNullOrEmpty(query.Keyword))
            expr = expr.Where(x => x.Code.Contains(query.Keyword) || x.Name.Contains(query.Keyword));
        if (query.ConfigType.HasValue)
            expr = expr.Where(x => x.ConfigType == query.ConfigType.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(x => x.SortOrder)
            .OrderByDescending(x => x.CreationTime)
            .ToPageListAsync(query.Page, query.PageSize, total);

        return ApiResult.Success(new PagedResponse<TestConfigViewDto>(
            list.Map<List<TestConfigViewDto>>(), total));
    }

    [OpenApiTag("temp/config")]
    [OpenApiOperation("获取测试配置详情", "根据ID获取测试配置")]
    [RoutePattern(pattern: "{id}", true)]
    public async Task<ApiResult<TestConfigViewDto>> GetAsync(long id)
    {
        var entity = await ConfigRepo.GetByIdAsync(id);
        if (entity == null)
            throw new UserFriendlyException("配置不存在");

        return ApiResult.Success(entity.Map<TestConfigViewDto>());
    }

    [OpenApiTag("temp/config")]
    [OpenApiOperation("创建测试配置", "创建新的测试配置")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync(
        [FromBody] TestConfigCreateCommand command)
    {
        var entity = command.Map<TestConfigEntity>();
        await ConfigRepo.InsertAsync(entity);
        return ApiResult.Success(entity.Id);
    }

    [OpenApiTag("temp/config")]
    [OpenApiOperation("更新测试配置", "更新已存在的测试配置")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult<bool>> UpdateAsync(
        [FromBody] TestConfigUpdateCommand command)
    {
        var entity = await ConfigRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("配置不存在");

        entity.Code = command.Code;
        entity.Name = command.Name;
        entity.Value = command.Value;
        entity.ConfigType = command.ConfigType;
        entity.IsEnabled = command.IsEnabled;
        entity.SortOrder = command.SortOrder;
        entity.Remark = command.Remark;

        await ConfigRepo.UpdateAsync(entity);
        return ApiResult.Success(true);
    }

    [OpenApiTag("temp/config")]
    [OpenApiOperation("删除测试配置", "删除测试配置")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await ConfigRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("配置不存在");

        await ConfigRepo.DeleteAsync(entity);
        return ApiResult.Success(true);
    }
}
