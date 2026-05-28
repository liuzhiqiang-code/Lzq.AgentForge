using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Commands;

/// <summary>
/// 创建质检单命令
/// </summary>
public record QCOrderCreateCommand
{
    /// <summary>质检类型：1-IQC 2-PQC 3-OQC</summary>
    public QCTypeEnum QCType { get; init; }

    /// <summary>关联单据编号（采购单号/工单编号/销售单号）</summary>
    public string? RefCode { get; init; }

    /// <summary>关联单据ID</summary>
    public long? RefId { get; init; }

    /// <summary>供应商ID（IQC时使用）</summary>
    public long? SupplierId { get; init; }

    /// <summary>供应商名称</summary>
    public string? SupplierName { get; init; }

    /// <summary>产品ID</summary>
    public long? ProductId { get; init; }

    /// <summary>产品名称</summary>
    public string? ProductName { get; init; }

    /// <summary>产品规格/型号</summary>
    public string? ProductSpec { get; init; }

    /// <summary>批号/批次</summary>
    public string? BatchNo { get; init; }

    /// <summary>送检数量</summary>
    public int SubmitQty { get; init; }

    /// <summary>质检标准/依据</summary>
    public string? QCStandard { get; init; }

    /// <summary>备注</summary>
    public string? Remark { get; init; }

    /// <summary>检验明细列表</summary>
    public List<QCOrderItemCreateCommand>? Items { get; init; }
}

/// <summary>
/// 质检明细创建命令
/// </summary>
public record QCOrderItemCreateCommand
{
    /// <summary>检验项目名称</summary>
    public string ItemName { get; init; } = string.Empty;

    /// <summary>检验项目类型：1-外观 2-尺寸 3-功能 4-性能 5-包装 6-其他</summary>
    public int ItemType { get; init; }

    /// <summary>检验标准/要求</summary>
    public string? Standard { get; init; }

    /// <summary>检验方法</summary>
    public string? Method { get; init; }

    /// <summary>抽样数量</summary>
    public int SampleQty { get; init; }
}
