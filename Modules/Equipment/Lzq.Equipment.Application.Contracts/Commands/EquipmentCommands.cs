using Lzq.Equipment.Domain.Enums;

namespace Lzq.Equipment.Application.Contracts.Commands;

/// <summary>
/// 创建设备命令
/// </summary>
public record EquipmentCreateCommand
{
    /// <summary>设备编号</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>设备名称</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>设备类型</summary>
    public EquipmentTypeEnum EquipmentType { get; init; }

    /// <summary>规格型号</summary>
    public string? Spec { get; init; }

    /// <summary>品牌/厂商</summary>
    public string? Brand { get; init; }

    /// <summary>供应商</summary>
    public string? Supplier { get; init; }

    /// <summary>购买日期</summary>
    public DateTime? PurchaseDate { get; init; }

    /// <summary>保修截止日期</summary>
    public DateTime? WarrantyEndDate { get; init; }

    /// <summary>所属产线ID</summary>
    public long? LineId { get; init; }

    /// <summary>所属产线名称</summary>
    public string? LineName { get; init; }

    /// <summary>安装位置</summary>
    public string? Location { get; init; }

    /// <summary>责任人ID</summary>
    public long? ResponsibleId { get; init; }

    /// <summary>责任人名称</summary>
    public string? ResponsibleName { get; init; }

    /// <summary>设备照片（JSON数组）</summary>
    public string? Photos { get; init; }

    /// <summary>设备参数（JSON）</summary>
    public string? Parameters { get; init; }

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}

/// <summary>
/// 更新设备命令
/// </summary>
public record EquipmentUpdateCommand
{
    /// <summary>设备ID</summary>
    public long Id { get; init; }

    /// <summary>设备名称</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>规格型号</summary>
    public string? Spec { get; init; }

    /// <summary>品牌/厂商</summary>
    public string? Brand { get; init; }

    /// <summary>供应商</summary>
    public string? Supplier { get; init; }

    /// <summary>购买日期</summary>
    public DateTime? PurchaseDate { get; init; }

    /// <summary>保修截止日期</summary>
    public DateTime? WarrantyEndDate { get; init; }

    /// <summary>所属产线ID</summary>
    public long? LineId { get; init; }

    /// <summary>所属产线名称</summary>
    public string? LineName { get; init; }

    /// <summary>安装位置</summary>
    public string? Location { get; init; }

    /// <summary>责任人ID</summary>
    public long? ResponsibleId { get; init; }

    /// <summary>责任人名称</summary>
    public string? ResponsibleName { get; init; }

    /// <summary>设备照片（JSON数组）</summary>
    public string? Photos { get; init; }

    /// <summary>设备参数（JSON）</summary>
    public string? Parameters { get; init; }

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}

/// <summary>
/// 更新设备状态命令
/// </summary>
public record EquipmentUpdateStatusCommand
{
    /// <summary>设备ID</summary>
    public long Id { get; init; }

    /// <summary>新状态</summary>
    public EquipmentStatusEnum Status { get; init; }
}
