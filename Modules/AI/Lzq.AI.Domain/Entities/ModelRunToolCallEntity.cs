using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.AI.Domain.Entities;

[Tenant("AgentForge"), SugarTable("ai_model_run_tool_call")]
public class ModelRunToolCallEntity : BaseFullEntity
{
    [SugarColumn(ColumnName = "run_record_id")]
    public long RunRecordId { get; set; }

    [SugarColumn(ColumnName = "call_id", Length = 100)]
    public string CallId { get; set; }

    [SugarColumn(ColumnName = "tool_name", Length = 200)]
    public string ToolName { get; set; }

    [SugarColumn(ColumnName = "skill_name", Length = 200)]
    public string? SkillName { get; set; }

    [SugarColumn(ColumnName = "tool_type", Length = 20)]
    public string ToolType { get; set; }

    [SugarColumn(ColumnName = "arguments", ColumnDataType = "longtext")]
    public string? Arguments { get; set; }

    [SugarColumn(ColumnName = "result", ColumnDataType = "longtext")]
    public string? Result { get; set; }

    [SugarColumn(ColumnName = "is_success")]
    public bool? IsSuccess { get; set; }

    [SugarColumn(ColumnName = "status", Length = 20)]
    public string Status { get; set; }

    [SugarColumn(ColumnName = "start_time")]
    public DateTime StartTime { get; set; }

    [SugarColumn(ColumnName = "end_time")]
    public DateTime? EndTime { get; set; }
}
