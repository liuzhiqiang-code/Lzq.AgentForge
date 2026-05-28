using Lzq.Extensions.SqlSugar.Entities;
using Lzq.MES.Domain.Enums;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

/// <summary>
/// 质检单实体 - 支持IQC/PQC/OQC三种质检类型
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_qc_order")]
public class QCOrderEntity : BaseFullEntity
{
    /// <summary>质检单编号</summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>质检类型：1-IQC来料检验 2-PQC过程检验 3-OQC出货检验</summary>
    [SugarColumn(ColumnName = "qc_type")]
    public QCTypeEnum QCType { get; set; }

    /// <summary>关联单据编号（如采购单号/工单编号/销售单号）</summary>
    [SugarColumn(ColumnName = "ref_code", Length = 100, IsNullable = true)]
    public string? RefCode { get; set; }

    /// <summary>关联单据ID（如采购单ID/工单ID）</summary>
    [SugarColumn(ColumnName = "ref_id", IsNullable = true)]
    public long? RefId { get; set; }

    /// <summary>供应商ID（IQC时使用）</summary>
    [SugarColumn(ColumnName = "supplier_id", IsNullable = true)]
    public long? SupplierId { get; set; }

    /// <summary>供应商名称</summary>
    [SugarColumn(ColumnName = "supplier_name", Length = 200, IsNullable = true)]
    public string? SupplierName { get; set; }

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

    /// <summary>送检数量</summary>
    [SugarColumn(ColumnName = "submit_qty")]
    public int SubmitQty { get; set; }

    /// <summary>合格数量</summary>
    [SugarColumn(ColumnName = "qualified_qty")]
    public int QualifiedQty { get; set; }

    /// <summary>不合格数量</summary>
    [SugarColumn(ColumnName = "unqualified_qty")]
    public int UnqualifiedQty { get; set; }

    /// <summary>质检单状态</summary>
    [SugarColumn(ColumnName = "status")]
    public QCOrderStatusEnum Status { get; set; } = QCOrderStatusEnum.Pending;

    /// <summary>检验员ID</summary>
    [SugarColumn(ColumnName = "inspector_id", IsNullable = true)]
    public long? InspectorId { get; set; }

    /// <summary>检验员名称</summary>
    [SugarColumn(ColumnName = "inspector_name", Length = 100, IsNullable = true)]
    public string? InspectorName { get; set; }

    /// <summary>检验日期</summary>
    [SugarColumn(ColumnName = "inspect_date", IsNullable = true)]
    public DateTime? InspectDate { get; set; }

    /// <summary>检验完成时间</summary>
    [SugarColumn(ColumnName = "completed_time", IsNullable = true)]
    public DateTime? CompletedTime { get; set; }

    /// <summary>质检标准/依据</summary>
    [SugarColumn(ColumnName = "qc_standard", Length = 1000, IsNullable = true)]
    public string? QCStandard { get; set; }

    /// <summary>检验结论</summary>
    [SugarColumn(ColumnName = "conclusion", Length = 2000, IsNullable = true)]
    public string? Conclusion { get; set; }

    /// <summary>备注</summary>
    [SugarColumn(ColumnName = "remark", Length = 2000, IsNullable = true)]
    public string? Remark { get; set; }
}
