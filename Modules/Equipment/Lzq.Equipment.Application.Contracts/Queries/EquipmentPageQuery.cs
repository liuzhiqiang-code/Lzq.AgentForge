using Lzq.Core.Models;
using Lzq.Equipment.Domain.Enums;

namespace Lzq.Equipment.Application.Contracts.Queries;

/// <summary>
/// 设备分页查询
/// </summary>
public record EquipmentPageQuery : PagedRequest
{
    /// <summary>设备编号</summary>
    public string? Code { get; init; }

    /// <summary>设备名称</summary>
    public string? Name { get; init; }

    /// <summary>设备类型</summary>
    public EquipmentTypeEnum? EquipmentType { get; init; }

    /// <summary>设备状态</summary>
    public EquipmentStatusEnum? Status { get; init; }

    /// <summary>产线ID</summary>
    public long? LineId { get; init; }

    /// <summary>责任人ID</summary>
    public long? ResponsibleId { get; init; }

    /// <summary>购买日期（起）</summary>
    public DateTime? PurchaseDateFrom { get; init; }

    /// <summary>购买日期（止）</summary>
    public DateTime? PurchaseDateTo { get; init; }

    /// <summary>创建时间（起）</summary>
    public DateTime? CreateTimeFrom { get; init; }

    /// <summary>创建时间（止）</summary>
    public DateTime? CreateTimeTo { get; init; }
}
