using Lzq.AI.Application.Contracts.Commands;
using Lzq.AI.Application.Contracts.Dtos;
using Lzq.AI.Application.Contracts.IServices;
using Lzq.AI.Application.Contracts.Queries;
using Lzq.AI.Domain.Consts;
using Lzq.AI.Domain.Entities;
using Lzq.AI.Domain.IRepositories;
using Lzq.Core.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SqlSugar;

namespace Lzq.AI.Application.Services;

public class AgentManageService : ServiceBase , IAgentManageService
{
    public AgentManageService() : base("/api/v1/ai/agentManage") { }
    private IAgentManageRepository AgentManageRepository => GetRequiredService<IAgentManageRepository>();

    [OpenApiTag("ai/agentManage"), OpenApiOperation("获取分页列表", "")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult> PageAsync([FromBody] AgentManagePageQuery query)
    {
        RefAsync<int> total = 0;
        var pageList = await AgentManageRepository.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Name  ), a => a.Name.Contains(query.Name))
            .ToPageListAsync(query.Page, query.PageSize, total);
        var result = pageList.Map<List<AgentManageViewDto>>();
        return ApiResult.Success(new PagedResponse<AgentManageViewDto>(result, total));
    }

    [OpenApiTag("ai/agentManage"), OpenApiOperation("获取列表", "")]
    [RoutePattern(pattern: "list", true)]
    public async Task<ApiResult> ListAsync()
    {
        var list = await AgentManageRepository.AsQueryable()
            .ToListAsync();
        return ApiResult.Success(list.Map<List<AgentManageViewDto>>());
    }

    [OpenApiTag("ai/agentManage"), OpenApiOperation("增加", "")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult> CreateAsync([FromBody] AgentManageCreateCommand command)
    {
        var entity = command.Map<AgentManageEntity>();
        await AgentManageRepository.InsertAsync(entity);
        return ApiResult.Success();
    }

    [OpenApiTag("ai/agentManage"), OpenApiOperation("更新", "")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult> UpdateAsync([FromBody] AgentManageUpdateCommand command)
    {
        var entity = command.Map<AgentManageEntity>();
        await AgentManageRepository.UpdateAsync(entity);
        return ApiResult.Success();
    }

    [OpenApiTag("ai/agentManage"), OpenApiOperation("删除", "")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult> DeleteAsync(long id)
    {
        await AgentManageRepository.DeleteAsync(a => id.Equals(a.Id));
        return ApiResult.Success();
    }

    [OpenApiTag("ai/agentManage"), OpenApiOperation("批量删除", "")]
    [RoutePattern(pattern: "batchDelete", true, HttpMethod = "Delete")]
    public async Task<ApiResult> BatchDeleteAsync([FromBody] List<long> ids)
    {
        await AgentManageRepository.DeleteAsync(a => ids.Contains(a.Id));
        return ApiResult.Success();
    }

    public async Task<AIAgentModel> GetAIAgentModel(string aiAgentName)
    {
        var agent = await AgentManageRepository.AsQueryable().FirstAsync(a => a.Name.Equals(aiAgentName));
        if (agent == null)
        {
            return AgentConst.CHAT;
        }
        return new AIAgentModel
        {
            Name = agent.Name,
            Description = agent.Description ?? "",
            ChatOptions = new Microsoft.Extensions.AI.ChatOptions
            {
                Instructions = agent.Instructions ?? "",
                Temperature = agent.Temperature,
                MaxOutputTokens = agent.MaxOutputTokens,
                TopP = agent.TopP,
                FrequencyPenalty = agent.FrequencyPenalty,
                PresencePenalty = agent.PresencePenalty
            },
            SelectedSkills = agent.SelectedSkills ?? []
        };
    }
}
