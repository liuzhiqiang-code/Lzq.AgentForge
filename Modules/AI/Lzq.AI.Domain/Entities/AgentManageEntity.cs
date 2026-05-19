using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.AI.Domain.Entities;

[Tenant("AgentForge"), SugarTable("ai_agent_manage")]
public class AgentManageEntity : BaseFullEntity
{
    /// <summary>
    /// 智能体名称
    /// </summary>
    [SugarColumn(ColumnName = "name", ColumnDescription = "智能体名称")]
    public string Name { get; set; }

    /// <summary>
    /// 智能体描述
    /// </summary>
    [SugarColumn(ColumnName = "description", ColumnDescription = "智能体描述")]
    public string? Description { get; set; }

    /// <summary>
    /// 智能体提示词
    /// </summary>
    [SugarColumn(ColumnName = "instructions", ColumnDescription = "智能体提示词")]
    public string? Instructions { get; set; }

    /// <summary>
    /// 温度控制
    /// </summary>
    [SugarColumn(ColumnName = "temperature", ColumnDescription = "温度控制")]
    public float? Temperature { get; set; }

    /// <summary>
    /// 最大输出token数
    /// </summary>
    [SugarColumn(ColumnName = "max_output_tokens", ColumnDescription = "最大输出token数")]
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// 核采样，控制多样性
    /// </summary>
    [SugarColumn(ColumnName = "top_p", ColumnDescription = "核采样，控制多样性")]
    public float? TopP { get; set; }

    /// <summary>
    /// 减少重复token
    /// </summary>
    [SugarColumn(ColumnName = "frequency_penalty", ColumnDescription = "减少重复token")]
    public float? FrequencyPenalty { get; set; }

    /// <summary>
    /// 减少已出现token
    /// </summary>
    [SugarColumn(ColumnName = "presence_penalty", ColumnDescription = "减少已出现token")]
    public float? PresencePenalty { get; set; }

    /// <summary>
    /// 绑定工具
    /// </summary>
    [SugarColumn(ColumnName = "selected_skills", IsJson = true, ColumnDescription = "绑定工具")]
    public List<SkillMethodEntry>? SelectedSkills { get; set; }

}