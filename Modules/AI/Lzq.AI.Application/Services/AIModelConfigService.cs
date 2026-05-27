using Lzq.AI.Application.Contracts.Commands;
using Lzq.AI.Application.Contracts.Dtos;
using Lzq.AI.Application.Contracts.IServices;
using Lzq.AI.Application.Contracts.Queries;
using Lzq.AI.Domain.Entities;
using Lzq.AI.Domain.IRepositories;
using Lzq.Core.Interfaces;
using Lzq.Core.Models;
using Lzq.Extensions.AI;
using Lzq.Extensions.AI.Interfaces;
using Masa.Utils.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSwag.Annotations;
using SqlSugar;
using System.Data;
using System.Text;

namespace Lzq.AI.Application.Services;

public class AIModelConfigService : ServiceBase, IAIModelConfigService
{
    public AIModelConfigService() : base("/api/v1/ai/modelConfig") { }
    private string AesKey => Convert.ToBase64String(Encoding.UTF8.GetBytes(
        Configuration.GetSection("AesKey").Get<string>() ?? ""
        ));
    private IConfiguration Configuration => GetRequiredService<IConfiguration>();
    private IAIAgentRunner AIAgentRunner => GetRequiredService<IAIAgentRunner>();
    private IModelConfigRepository ModelConfigRepository => GetRequiredService<IModelConfigRepository>();
    private IApiKeyRepository ApiKeyRepository => GetRequiredService<IApiKeyRepository>();
    private IUnitOfWork UnitOfWork => GetRequiredService<IUnitOfWork>();

    [OpenApiTag("ai/modelConfig"), OpenApiOperation("获取分页列表", "")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult> PageAsync([FromBody] ModelConfigPageQuery query)
    {
        RefAsync<int> total = 0;
        var pageList = await ModelConfigRepository.AsQueryable()
            .LeftJoin<ApiKeyEntity>((a, b) => a.ApiKeyId == b.Id)
            .Where((a, b) => b.IsEnabled)
            .OrderByDescending((a, b) => a.IsEnabled)
            .OrderBy((a, b) => b.Provider)
            .OrderBy((a, b) => b.KeyName)
            .OrderBy((a, b) => a.ModificationTime)
            .Select((a, b) => new ModelConfigViewDto
            {
                Id = a.Id,
                ApiKeyId = a.ApiKeyId,
                ConfigName = a.ConfigName,
                DisplayModelName = a.DisplayModelName,
                ContextLength = a.ContextLength,
                IsEnabled = a.IsEnabled,
                Provider = b.Provider,
                KeyName = b.KeyName
            })
            .ToPageListAsync(query.Page, query.PageSize, total);
        return ApiResult.Success(new PagedResponse<ModelConfigViewDto>(pageList, total));
    }

    [OpenApiTag("ai/modelConfig"), OpenApiOperation("获取列表", "")]
    [RoutePattern(pattern: "list", true)]
    public async Task<ApiResult> ListAsync()
    {
        var list = await ModelConfigRepository.AsQueryable()
            .LeftJoin<ApiKeyEntity>((a, b) => a.ApiKeyId == b.Id)
            .OrderByDescending((a, b) => a.IsEnabled)
            .OrderBy((a, b) => b.Provider)
            .OrderBy((a, b) => b.KeyName)
            .OrderBy((a, b) => a.ModificationTime)
            .Select((a, b) => new ModelConfigViewDto {
                Id = a.Id,
                ApiKeyId = a.ApiKeyId,
                ConfigName = a.ConfigName,
                DisplayModelName = a.DisplayModelName,
                ContextLength = a.ContextLength,
                IsEnabled = a.IsEnabled,
                Provider = b.Provider,
                KeyName = b.KeyName
            })
            .ToListAsync();
        return ApiResult.Success(list);
    }

    [OpenApiTag("ai/modelConfig"), OpenApiOperation("增加", "")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult> CreateAsync([FromBody] ModelConfigCreateCommand command)
    {
        var entity = command.Map<ModelConfigEntity>();
        var apiKey = await ApiKeyRepository.GetByIdAsync(command.ApiKeyId);
        try
        {
            await UnitOfWork.BeginTranAsync(IsolationLevel.ReadCommitted);
            await ModelConfigRepository.InsertAsync(entity);
            var setting = new AISetting
            {
                ConfigId = apiKey.KeyName + "_" + command.ConfigName + "_" + entity.Id,
                Url = apiKey.BaseUrl,
                Model = command.DisplayModelName,
                KeySecret = AesUtils.Decrypt(apiKey.KeyValue, AesKey, apiKey.KeyIv)
            };
            var agentModel = new AIAgentModel { Name = "测试链接" };
            await AIAgentRunner.RunAsync(setting, agentModel, "Ping");
            await UnitOfWork.CommitTranAsync();
            return ApiResult.Success();
        }
        catch (Exception)
        {
            await UnitOfWork.RollbackTranAsync();
            throw new UserFriendlyException("AI模型配置测试连接失败");
        }
    }

    [OpenApiTag("ai/modelConfig"), OpenApiOperation("更新", "")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult> UpdateAsync([FromBody] ModelConfigUpdateCommand command)
    {
        var entity = await ModelConfigRepository.GetByIdAsync(command.Id);
        entity.ConfigName = command.ConfigName;
        entity.ContextLength = command.ContextLength;
        entity.IsEnabled = command.IsEnabled;
        if (entity.ConfigName.IsNullOrWhiteSpace())
            entity.ConfigName = entity.DisplayModelName;
        await ModelConfigRepository.UpdateAsync(entity);
        return ApiResult.Success();
    }

    [OpenApiTag("ai/modelConfig"), OpenApiOperation("删除", "")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult> DeleteAsync(long id)
    {
        await ModelConfigRepository.DeleteAsync(a => id.Equals(a.Id));
        return ApiResult.Success();
    }

    [OpenApiTag("ai/modelConfig"), OpenApiOperation("批量删除", "")]
    [RoutePattern(pattern: "batchDelete", true, HttpMethod = "Delete")]
    public async Task<ApiResult> BatchDeleteAsync([FromBody] List<long> ids)
    {
        await ModelConfigRepository.DeleteAsync(a => ids.Contains(a.Id));
        return ApiResult.Success();
    }
}
