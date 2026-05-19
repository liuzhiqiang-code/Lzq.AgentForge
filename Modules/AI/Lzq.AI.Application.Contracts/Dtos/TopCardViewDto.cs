namespace Lzq.AI.Application.Contracts.Dtos;

public record TopCardViewDto
{
    /// <summary>
    /// 平台累计对话总数（所有对话会话数）
    /// </summary>
    public int TotalConversations { get; set; }

    /// <summary>
    /// 今日新建的对话数
    /// </summary>
    public int TodayConversations { get; set; }

    /// <summary>
    /// 当前启用状态的智能体数量
    /// </summary>
    public int ActiveAgents { get; set; }

    /// <summary>
    /// 智能体总数（包含启用和禁用）
    /// </summary>
    public int TotalAgents { get; set; }

    /// <summary>
    /// 今日模型 API 调用次数（或可改为 Token 消耗量）
    /// </summary>
    public int TodayApiCalls { get; set; }

    /// <summary>
    /// 累计模型 API 调用次数
    /// </summary>
    public int TotalApiCalls { get; set; }

    /// <summary>
    /// 今日技能被调用的总次数
    /// </summary>
    public int TodaySkillCalls { get; set; }
}
