using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.AI.Domain.Entities;

[Tenant("AgentForge"), SugarTable("ai_chats")]
public class ChatsEntity : BaseFullEntity
{
    /// <summary>会话标题（AI 自动生成或用户修改）</summary>
    [SugarColumn(ColumnName = "title", Length = 200)]
    public string Title { get; set; }

    /// <summary>使用的聊天客户端模型标识（如 DeepSeekChat）</summary>
    [SugarColumn(ColumnName = "chat_client", Length = 100)]
    public string ChatClient { get; set; }

    /// <summary>使用的智能体名称</summary>
    [SugarColumn(ColumnName = "ai_agent_name", Length = 100)]
    public string AIAgentName { get; set; }

    /// <summary>最后一条 AI 回复的截断内容，用于列表预览</summary>
    [SugarColumn(ColumnName = "last_message", Length = 500)]
    public string? LastMessage { get; set; }

    /// <summary>是否置顶</summary>
    [SugarColumn(ColumnName = "is_top")]
    public bool IsTop { get; set; } = false;

    [SugarColumn(ColumnName = "session_id")]
    public string? SessionId { get; set; }

    /// <summary>
    /// 绑定工具   冗余绑定工具，智能体修改的绑定工具新对话才会更新，已存在的对话不更新
    /// </summary>
    [SugarColumn(ColumnName = "selected_skills", IsJson = true, ColumnDescription = "绑定工具")]
    public List<SkillMethodEntry> SelectedSkills { get; set; } = [];
}
