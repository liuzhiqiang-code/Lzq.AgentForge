using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Dtos;

/// <summary>
/// 质检单视图DTO
/// </summary>
public class QCOrderViewDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public QCTypeEnum QCType { get; set; }
    public string? QCTypeName { get; set; }
    public string? RefCode { get; set; }
    public long? RefId { get; set; }
    public long? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public long? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSpec { get; set; }
    public string? BatchNo { get; set; }
    public int SubmitQty { get; set; }
    public int QualifiedQty { get; set; }
    public int UnqualifiedQty { get; set; }
    public QCOrderStatusEnum Status { get; set; }
    public string? StatusName { get; set; }
    public long? InspectorId { get; set; }
    public string? InspectorName { get; set; }
    public DateTime? InspectDate { get; set; }
    public DateTime? CompletedTime { get; set; }
    public string? QCStandard { get; set; }
    public string? Conclusion { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public List<QCOrderItemViewDto>? Items { get; set; }
}

/// <summary>
/// 质检明细视图DTO
/// </summary>
public class QCOrderItemViewDto
{
    public long Id { get; set; }
    public long QCOrderId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int ItemType { get; set; }
    public string? ItemTypeName { get; set; }
    public string? Standard { get; set; }
    public string? Method { get; set; }
    public int SampleQty { get; set; }
    public int QualifiedQty { get; set; }
    public int UnqualifiedQty { get; set; }
    public QCResultEnum Result { get; set; }
    public string? ResultName { get; set; }
    public string? DefectDesc { get; set; }
    public string? DefectCode { get; set; }
    public string? Remark { get; set; }
}

