namespace Lzq.AI.Application.Contracts.IServices;

public interface IAnalyticsService
{
    Task<ApiResult> TopCardAsync();
    Task<ApiResult> GetConversationTrendsAsync(int days = 7);
    Task<ApiResult> GetModelUsageMonthlyAsync(int months = 6);
    Task<ApiResult> GetAgentUsageRankingAsync(int top = 5);
    Task<ApiResult> GetModelDistributionAsync();
    Task<ApiResult> GetSkillStatsAsync();
}
