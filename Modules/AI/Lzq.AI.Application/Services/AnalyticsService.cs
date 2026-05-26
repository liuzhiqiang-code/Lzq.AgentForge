using Lzq.AI.Application.Contracts.Dtos;
using Lzq.AI.Application.Contracts.IServices;
using Lzq.AI.Domain.IRepositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SqlSugar;

namespace Lzq.AI.Application.Services;

public class AnalyticsService : ServiceBase, IAnalyticsService
{
    public AnalyticsService() : base("/api/v1/ai/analytics") { }

    private IAgentManageRepository AgentManageRepository => GetRequiredService<IAgentManageRepository>();
    private IChatsRepository ChatsRepository => GetRequiredService<IChatsRepository>();
    private IModelRunRecordRepository ModelRunRecordRepository => GetRequiredService<IModelRunRecordRepository>();
    private IModelRunToolCallRepository ModelRunToolCallRepository => GetRequiredService<IModelRunToolCallRepository>();

    [OpenApiTag("ai/analytics"), OpenApiOperation("获取顶部卡片数据信息", "")]
    [RoutePattern(pattern: "topCard", true, HttpMethod = "Get")]
    public async Task<ApiResult> TopCardAsync()
    {
        var todayStart = DateTime.Today;
        var todayEnd = todayStart.AddDays(1);

        var totalConversationsTask = ChatsRepository.AsQueryable().CountAsync();
        var todayConversationsTask = ChatsRepository.AsQueryable().CountAsync(c => c.CreationTime >= todayStart && c.CreationTime < todayEnd);
        var totalAgentsTask = AgentManageRepository.AsQueryable().CountAsync();
        var activeAgentsTask = ChatsRepository.AsQueryable()
            .Where(c => c.CreationTime >= todayStart)
            .Select(c => c.AIAgentName).Distinct().CountAsync();
        var todayApiCallsTask = ModelRunRecordRepository.AsQueryable().CountAsync(r => r.CreationTime >= todayStart && r.CreationTime < todayEnd);
        var totalApiCallsTask = ModelRunRecordRepository.AsQueryable().CountAsync();

        var todaySkillCallsTask = ModelRunToolCallRepository.AsQueryable()
            .CountAsync(t => t.StartTime >= todayStart && t.StartTime < todayEnd && t.Status == "done");
        var totalSkillCallsTask = ModelRunToolCallRepository.AsQueryable()
            .CountAsync(t => t.Status == "done");

        await Task.WhenAll(totalConversationsTask, todayConversationsTask, totalAgentsTask, activeAgentsTask, todayApiCallsTask, totalApiCallsTask, todaySkillCallsTask, totalSkillCallsTask);

        var result = new TopCardViewDto
        {
            TotalConversations = totalConversationsTask.Result,
            TodayConversations = todayConversationsTask.Result,
            ActiveAgents = activeAgentsTask.Result,
            TotalAgents = totalAgentsTask.Result,
            TodayApiCalls = todayApiCallsTask.Result,
            TotalApiCalls = totalApiCallsTask.Result,
            TodaySkillCalls = todaySkillCallsTask.Result,
            TotalSkillCalls = totalSkillCallsTask.Result,
        };

        return ApiResult.Success(result);
    }

    [OpenApiTag("ai/analytics"), OpenApiOperation("获取对话请求趋势", "")]
    [RoutePattern(pattern: "conversation-trends", true, HttpMethod = "Get")]
    public async Task<ApiResult> GetConversationTrendsAsync([FromQuery] int days = 7)
    {
        var startDate = DateTime.Today.AddDays(-(days - 1));
        var dates = Enumerable.Range(0, days)
            .Select(i => startDate.AddDays(i).ToString("MM-dd"))
            .ToList();

        var conversationsPerDay = await ChatsRepository.AsQueryable()
            .Where(c => c.CreationTime >= startDate)
            .GroupBy(c => c.CreationTime.Date)
            .Select(c => new { Date = c.CreationTime.Date, Count = SqlFunc.AggregateCount(c.Id) })
            .ToListAsync();

        var callsPerDay = await ModelRunRecordRepository.AsQueryable()
            .Where(r => r.CreationTime >= startDate)
            .GroupBy(r => r.CreationTime.Date)
            .Select(r => new { Date = r.CreationTime.Date, Count = SqlFunc.AggregateCount(r.Id) })
            .ToListAsync();

        var convDict = conversationsPerDay.ToDictionary(x => x.Date.ToString("MM-dd"), x => (int)x.Count);
        var callDict = callsPerDay.ToDictionary(x => x.Date.ToString("MM-dd"), x => (int)x.Count);

        var result = new ConversationTrendsDto
        {
            Dates = dates,
            Conversations = dates.Select(d => convDict.GetValueOrDefault(d, 0)).ToArray(),
            ApiCalls = dates.Select(d => callDict.GetValueOrDefault(d, 0)).ToArray()
        };

        return ApiResult.Success(result);
    }

    [OpenApiTag("ai/analytics"), OpenApiOperation("获取模型调用量月度统计", "")]
    [RoutePattern(pattern: "model-usage-monthly", true, HttpMethod = "Get")]
    public async Task<ApiResult> GetModelUsageMonthlyAsync([FromQuery] int months = 6)
    {
        var startMonth = DateTime.Today.AddMonths(-(months - 1));
        var monthLabels = Enumerable.Range(0, months)
            .Select(i => startMonth.AddMonths(i).ToString("yyyy-MM"))
            .ToList();

        var usageByMonth = await ModelRunRecordRepository.AsQueryable()
            .Where(r => r.CreationTime >= startMonth)
            .GroupBy(r => new { r.CreationTime.Year, r.CreationTime.Month })
            .Select(r => new { r.CreationTime.Year, r.CreationTime.Month, Count = SqlFunc.AggregateCount(r.Id) })
            .ToListAsync();

        var monthDict = usageByMonth.ToDictionary(x => $"{x.Year}-{x.Month:D2}", x => (int)x.Count);

        var result = new ModelUsageMonthlyDto
        {
            Months = monthLabels.Select(m => $"{m[..4]}年{m[5..7]}月").ToArray(),
            Values = monthLabels.Select(m => monthDict.GetValueOrDefault(m, 0)).ToArray()
        };

        return ApiResult.Success(result);
    }

    [OpenApiTag("ai/analytics"), OpenApiOperation("获取智能体使用排行", "")]
    [RoutePattern(pattern: "agent-usage-ranking", true, HttpMethod = "Get")]
    public async Task<ApiResult> GetAgentUsageRankingAsync([FromQuery] int top = 5)
    {
        var ranking = await ModelRunRecordRepository.AsQueryable()
            .GroupBy(r => r.AIAgentName)
            .Select(r => new { Name = r.AIAgentName, Count = SqlFunc.AggregateCount(r.Id) })
            .ToListAsync();

        var result = ranking
            .OrderByDescending(x => x.Count)
            .Take(top)
            .Select(x => new AgentUsageItemDto { Name = x.Name, Count = (int)x.Count })
            .ToList();

        return ApiResult.Success(result);
    }

    [OpenApiTag("ai/analytics"), OpenApiOperation("获取模型调用分布", "")]
    [RoutePattern(pattern: "model-distribution", true, HttpMethod = "Get")]
    public async Task<ApiResult> GetModelDistributionAsync()
    {
        var distribution = await ModelRunRecordRepository.AsQueryable()
            .GroupBy(r => r.ChatClient)
            .Select(r => new { Name = r.ChatClient, Count = SqlFunc.AggregateCount(r.Id) })
            .ToListAsync();

        var result = distribution
            .Select(x => new ModelDistributionItemDto { Name = x.Name, Value = (int)x.Count })
            .ToList();

        return ApiResult.Success(result);
    }

    [OpenApiTag("ai/analytics"), OpenApiOperation("获取技能使用统计", "")]
    [RoutePattern(pattern: "skill-stats", true, HttpMethod = "Get")]
    public async Task<ApiResult> GetSkillStatsAsync()
    {
        var stats = await ModelRunToolCallRepository.AsQueryable()
            .Where(t => t.SkillName != null && t.Status == "done")
            .GroupBy(t => t.SkillName)
            .Select(t => new { Name = t.SkillName, Count = SqlFunc.AggregateCount(t.Id) })
            .ToListAsync();

        var result = stats
            .OrderByDescending(x => x.Count)
            .Select(x => new SkillStatsItemDto { Name = x.Name!, Count = (int)x.Count })
            .ToList();

        return ApiResult.Success(result);
    }
}
