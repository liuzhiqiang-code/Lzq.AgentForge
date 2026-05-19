using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.WorkOrder.Domain.Entities.WorkOrder;

/// <summary>
/// 报工记录实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_work_report")]
public class WorkReportEntity : BaseFullEntity
{
    /// <summary>工单ID</summary>
    [SugarColumn(ColumnName = "work_order_id")]
    public long WorkOrderId { get; set; }

    /// <summary>操作人员ID</summary>
    [SugarColumn(ColumnName = "operator_id", Length = 100, IsNullable = true)]
    public string? OperatorId { get; set; }

    /// <summary>操作人员名称</summary>
    [SugarColumn(ColumnName = "operator_name", Length = 100, IsNullable = true)]
    public string? OperatorName { get; set; }

    /// <summary>合格数量</summary>
    [SugarColumn(ColumnName = "qualified_qty")]
    public int QualifiedQty { get; set; }

    /// <summary>不良数量</summary>
    [SugarColumn(ColumnName = "defect_qty")]
    public int DefectQty { get; set; }

    /// <summary>工时（小时）</summary>
    [SugarColumn(ColumnName = "work_hours", DecimalDigits = 2)]
    public decimal WorkHours { get; set; }

    /// <summary>报工时间</summary>
    [SugarColumn(ColumnName = "report_time")]
    public DateTime ReportTime { get; set; }

    /// <summary>班次（白班/夜班）</summary>
    [SugarColumn(ColumnName = "shift", Length = 20, IsNullable = true)]
    public string? Shift { get; set; }

    /// <summary>备注</summary>
    [SugarColumn(ColumnName = "remark", Length = 500, IsNullable = true)]
    public string? Remark { get; set; }
}
