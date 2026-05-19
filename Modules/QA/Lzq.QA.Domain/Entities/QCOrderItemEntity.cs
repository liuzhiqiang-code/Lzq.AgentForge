using Lzq.Extensions.SqlSugar.Entities;
using Lzq.QA.Domain.Enums;
using SqlSugar;

namespace Lzq.QA.Domain.Entities;

/// <summary>
/// 质检明细实体 - 记录每一条检验项目的结果
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_qc_order_item")]
public class QCOrderItemEntity : BaseFullEntity
{
    /// <summary>质检单ID</summary>
    [SugarColumn(ColumnName = "qc_order_id")]
    public long QCOrderId { get; set; }

    /// <summary>检验项目名称</summary>
    [SugarColumn(ColumnName = "item_name", Length = 200)]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>检验项目类型：1-外观 2-尺寸 3-功能 4-性能 5-包装 6-其他</summary>
    [SugarColumn(ColumnName = "item_type")]
    public int ItemType { get; set; }

    /// <summary>检验标准/要求</summary>
    [SugarColumn(ColumnName = "standard", Length = 500, IsNullable = true)]
    public string? Standard { get; set; }

    /// <summary>检验方法</summary>
    [SugarColumn(ColumnName = "method", Length = 500, IsNullable = true)]
    public string? Method { get; set; }

    /// <summary>抽样数量</summary>
    [SugarColumn(ColumnName = "sample_qty")]
    public int SampleQty { get; set; }

    /// <summary>合格数量</summary>
    [SugarColumn(ColumnName = "qualified_qty")]
    public int QualifiedQty { get; set; }

    /// <summary>不合格数量</summary>
    [SugarColumn(ColumnName = "unqualified_qty")]
    public int UnqualifiedQty { get; set; }

    /// <summary>检验结果：1-合格 2-不合格 3-让步接收</summary>
    [SugarColumn(ColumnName = "result")]
    public QCResultEnum Result { get; set; }

    /// <summary>不合格描述</summary>
    [SugarColumn(ColumnName = "defect_desc", Length = 1000, IsNullable = true)]
    public string? DefectDesc { get; set; }

    /// <summary>不合格代码（可选，用于关联不良品处理）</summary>
    [SugarColumn(ColumnName = "defect_code", Length = 50, IsNullable = true)]
    public string? DefectCode { get; set; }

    /// <summary>备注</summary>
    [SugarColumn(ColumnName = "remark", Length = 500, IsNullable = true)]
    public string? Remark { get; set; }
}
