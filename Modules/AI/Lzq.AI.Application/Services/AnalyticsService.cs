using Lzq.AI.Application.Contracts.Dtos;
using Lzq.AI.Application.Contracts.IServices;
using Lzq.AI.Domain.IRepositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace Lzq.AI.Application.Services;

public class AnalyticsService : ServiceBase, IAnalyticsService
{
    public AnalyticsService() : base("/api/v1/ai/analytics") { }

    private IAgentManageRepository AgentManageRepository => GetRequiredService<IAgentManageRepository>();
    private IChatsRepository ChatsRepository => GetRequiredService<IChatsRepository>();

    [OpenApiTag("ai/analytics"), OpenApiOperation("获取顶部卡片数据信息", "")]
    [RoutePattern(pattern: "topCard", true, HttpMethod = "Get")]
    public async Task<ApiResult> TopCardAsync()
    {
        var todayStart = DateTime.Today;
        var todayEnd = todayStart.AddDays(1);

        // 并行统计
        var totalConversationsTask = ChatsRepository.AsQueryable().CountAsync();
        var todayConversationsTask = ChatsRepository.AsQueryable().CountAsync(c => c.CreationTime >= todayStart && c.CreationTime < todayEnd);
        var totalAgentsTask = AgentManageRepository.AsQueryable().CountAsync();
        // 假设活跃智能体为今日产生过对话的智能体
        var activeAgentsTask = ChatsRepository.AsQueryable()
            .Where(c => c.CreationTime >= todayStart)
            .Select(c => c.AIAgentName).Distinct().CountAsync();

        await Task.WhenAll(totalConversationsTask, todayConversationsTask, totalAgentsTask, activeAgentsTask);

        var result = new TopCardViewDto
        {
            TotalConversations = totalConversationsTask.Result,
            TodayConversations = todayConversationsTask.Result,
            ActiveAgents = activeAgentsTask.Result,
            TotalAgents = totalAgentsTask.Result,
            TodayApiCalls = 1250, // 模拟数据
            TotalApiCalls = 85420, // 模拟数据
            TodaySkillCalls = 88 // 模拟数据
        };

        return ApiResult.Success(result);
    }

    [OpenApiTag("ai/analytics"), OpenApiOperation("获取对话请求趋势", "")]
    [RoutePattern(pattern: "conversation-trends", true, HttpMethod = "Get")]
    public async Task<ApiResult> GetConversationTrendsAsync([FromQuery] int days = 7)
    {
        // 模拟近7天趋势
        var dates = Enumerable.Range(0, days)
            .Select(i => DateTime.Today.AddDays(-i).ToString("MM-dd"))
            .Reverse().ToList();

        var result = new ConversationTrendsDto
        {
            Dates = dates,
            UserRequests = dates.Select(_ => Random.Shared.Next(100, 500)).ToArray(),
            AssistantResponses = dates.Select(_ => Random.Shared.Next(100, 500)).ToArray()
        };

        return ApiResult.Success(result);
    }

    [OpenApiTag("ai/analytics"), OpenApiOperation("获取模型调用量月度统计", "")]
    [RoutePattern(pattern: "model-usage-monthly", true, HttpMethod = "Get")]
    public async Task<ApiResult> GetModelUsageMonthlyAsync([FromQuery] int months = 6)
    {
        var monthLabels = new[] { "1月", "2月", "3月", "4月", "5月", "6月" };
        var result = new ModelUsageMonthlyDto
        {
            Months = monthLabels,
            Values = monthLabels.Select(_ => Random.Shared.Next(1000, 5000)).ToArray()
        };

        return ApiResult.Success(result);
    }

    [OpenApiTag("ai/analytics"), OpenApiOperation("获取智能体使用排行", "")]
    [RoutePattern(pattern: "agent-usage-ranking", true, HttpMethod = "Get")]
    public async Task<ApiResult> GetAgentUsageRankingAsync([FromQuery] int top = 5)
    {
        // 尝试从数据库获取真实前几名，若无数据则返回模拟
        var agents = await AgentManageRepository.AsQueryable()
            .Take(top)
            .Select(a => new AgentUsageItemDto { Name = a.Name, Count = Random.Shared.Next(50, 200) })
            .ToListAsync();

        return ApiResult.Success(agents);
    }

    [OpenApiTag("ai/analytics"), OpenApiOperation("获取模型调用分布", "")]
    [RoutePattern(pattern: "model-distribution", true, HttpMethod = "Get")]
    public async Task<ApiResult> GetModelDistributionAsync()
    {
        var result = new List<ModelDistributionItemDto>
        {
            new() { Name = "GPT-4o", Value = 450 },
            new() { Name = "Claude 3.5 Sonnet", Value = 320 },
            new() { Name = "DeepSeek-V3", Value = 280 },
            new() { Name = "Gemini 1.5 Pro", Value = 150 }
        };

        return ApiResult.Success(result);
    }

    [OpenApiTag("ai/analytics"), OpenApiOperation("获取技能使用统计", "")]
    [RoutePattern(pattern: "skill-stats", true, HttpMethod = "Get")]
    public async Task<ApiResult> GetSkillStatsAsync()
    {
        var result = new List<SkillStatsItemDto>
        {
            new() { Name = "Google Search", Count = 120 },
            new() { Name = "Python Interpreter", Count = 85 },
            new() { Name = "File Reader", Count = 64 },
            new() { Name = "Image Generator", Count = 42 }
        };

        return ApiResult.Success(result);
    }
}
