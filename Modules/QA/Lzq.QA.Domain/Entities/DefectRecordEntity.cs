using Lzq.Extensions.SqlSugar.Entities;
using Lzq.QA.Domain.Enums;
using SqlSugar;

namespace Lzq.QA.Domain.Entities;

/// <summary>
/// 不良品记录实体 - 记录不合格品的处理信息
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_defect_record")]
public class DefectRecordEntity : BaseFullEntity
{
    /// <summary>质检单ID</summary>
    [SugarColumn(ColumnName = "qc_order_id", IsNullable = true)]
    public long? QCOrderId { get; set; }

    /// <summary>质检单编号</summary>
    [SugarColumn(ColumnName = "qc_order_code", Length = 50, IsNullable = true)]
    public string? QCOrderCode { get; set; }

    /// <summary>关联工单ID（可选）</summary>
    [SugarColumn(ColumnName = "work_order_id", IsNullable = true)]
    public long? WorkOrderId { get; set; }

    /// <summary>关联工单编号（可选）</summary>
    [SugarColumn(ColumnName = "work_order_code", Length = 50, IsNullable = true)]
    public string? WorkOrderCode { get; set; }

    /// <summary>产品ID</summary>
    [SugarColumn(ColumnName = "product_id", IsNullable = true)]
    public long? ProductId { get; set; }

    /// <summary>产品名称</summary>
    [SugarColumn(ColumnName = "product_name", Length = 200, IsNullable = true)]
    public string? ProductName { get; set; }

    /// <summary>产品规格/型号</summary>
    [SugarColumn(ColumnName = "product_spec", Length = 200, IsNullable = true)]
    public string? ProductSpec { get; set; }

    /// <summary>批号/批次</summary>
    [SugarColumn(ColumnName = "batch_no", Length = 100, IsNullable = true)]
    public string? BatchNo { get; set; }

    /// <summary>不良品数量</summary>
    [SugarColumn(ColumnName = "defect_qty")]
    public int DefectQty { get; set; }

    /// <summary>不合格代码/类型</summary>
    [SugarColumn(ColumnName = "defect_code", Length = 50, IsNullable = true)]
    public string? DefectCode { get; set; }

    /// <summary>不合格描述</summary>
    [SugarColumn(ColumnName = "defect_desc", Length = 1000, IsNullable = true)]
    public string? DefectDesc { get; set; }

    /// <summary>不合格图片（JSON数组存储）</summary>
    [SugarColumn(ColumnName = "defect_images", Length = 4000, IsNullable = true)]
    public string? DefectImages { get; set; }

    /// <summary>不良品状态</summary>
    [SugarColumn(ColumnName = "status")]
    public DefectStatusEnum Status { get; set; }

    /// <summary>处理方式</summary>
    [SugarColumn(ColumnName = "handling_type", IsNullable = true)]
    public DefectHandlingEnum? HandlingType { get; set; }

    /// <summary>处理说明</summary>
    [SugarColumn(ColumnName = "handling_remark", Length = 2000, IsNullable = true)]
    public string? HandlingRemark { get; set; }

    /// <summary>处理人ID</summary>
    [SugarColumn(ColumnName = "handler_id", IsNullable = true)]
    public long? HandlerId { get; set; }

    /// <summary>处理人名称</summary>
    [SugarColumn(ColumnName = "handler_name", Length = 100, IsNullable = true)]
    public string? HandlerName { get; set; }

    /// <summary>处理时间</summary>
    [SugarColumn(ColumnName = "handling_time", IsNullable = true)]
    public DateTime? HandlingTime { get; set; }

    /// <summary>是否需要评审</summary>
    [SugarColumn(ColumnName = "need_review")]
    public bool NeedReview { get; set; }

    /// <summary>评审结果</summary>
    [SugarColumn(ColumnName = "review_result", Length = 1000, IsNullable = true)]
    public string? ReviewResult { get; set; }

    /// <summary>评审人</summary>
    [SugarColumn(ColumnName = "reviewer", Length = 100, IsNullable = true)]
    public string? Reviewer { get; set; }

    /// <summary>评审时间</summary>
    [SugarColumn(ColumnName = "review_time", IsNullable = true)]
    public DateTime? ReviewTime { get; set; }

    /// <summary>备注</summary>
    [SugarColumn(ColumnName = "remark", Length = 2000, IsNullable = true)]
    public string? Remark { get; set; }
}
