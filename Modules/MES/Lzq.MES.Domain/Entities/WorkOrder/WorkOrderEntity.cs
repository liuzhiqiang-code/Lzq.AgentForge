using Lzq.Extensions.SqlSugar.Entities;
using Lzq.MES.Domain.Enums;
using SqlSugar;

namespace Lzq.MES.Domain.Entities.WorkOrder;

/// <summary>
/// 工单实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_work_order")]
public class WorkOrderEntity : BaseFullEntity
{
    /// <summary>工单编号</summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>产品名称</summary>
    [SugarColumn(ColumnName = "product_name", Length = 200)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>产品规格/型号</summary>
    [SugarColumn(ColumnName = "product_spec", Length = 200, IsNullable = true)]
    public string? ProductSpec { get; set; }

    /// <summary>产线ID</summary>
    [SugarColumn(ColumnName = "line_id")]
    public long LineId { get; set; }

    /// <summary>工序ID</summary>
    [SugarColumn(ColumnName = "process_id")]
    public long ProcessId { get; set; }

    /// <summary>计划数量</summary>
    [SugarColumn(ColumnName = "planned_qty")]
    public int PlannedQty { get; set; }

    /// <summary>已完成数量</summary>
    [SugarColumn(ColumnName = "completed_qty")]
    public int CompletedQty { get; set; }

    /// <summary>不良数量</summary>
    [SugarColumn(ColumnName = "defect_qty")]
    public int DefectQty { get; set; }

    /// <summary>工单状态</summary>
    [SugarColumn(ColumnName = "status")]
    public WorkOrderStatusEnum Status { get; set; } = WorkOrderStatusEnum.Draft;

    /// <summary>计划开始时间</summary>
    [SugarColumn(ColumnName = "planned_start", IsNullable = true)]
    public DateTime? PlannedStart { get; set; }

    /// <summary>计划结束时间</summary>
    [SugarColumn(ColumnName = "planned_end", IsNullable = true)]
    public DateTime? PlannedEnd { get; set; }

    /// <summary>实际开始时间</summary>
    [SugarColumn(ColumnName = "actual_start", IsNullable = true)]
    public DateTime? ActualStart { get; set; }

    /// <summary>实际结束时间</summary>
    [SugarColumn(ColumnName = "actual_end", IsNullable = true)]
    public DateTime? ActualEnd { get; set; }

    /// <summary>优先级（1-5，5最高）</summary>
    [SugarColumn(ColumnName = "priority")]
    public int Priority { get; set; } = 3;

    /// <summary>备注</summary>
    [SugarColumn(ColumnName = "remark", Length = 2000, IsNullable = true)]
    public string? Remark { get; set; }
}
