using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Dtos;

/// <summary>
/// 设备视图DTO
/// </summary>
public class EquipmentViewDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public EquipmentTypeEnum EquipmentType { get; set; }
    public string? EquipmentTypeName { get; set; }
    public string? Spec { get; set; }
    public string? Brand { get; set; }
    public string? Supplier { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public EquipmentStatusEnum Status { get; set; }
    public string? StatusName { get; set; }
    public long? LineId { get; set; }
    public string? LineName { get; set; }
    public string? Location { get; set; }
    public long? ResponsibleId { get; set; }
    public string? ResponsibleName { get; set; }
    public string? Photos { get; set; }
    public string? Parameters { get; set; }
    public decimal TotalRunningHours { get; set; }
    public int TotalRepairCount { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}

/// <summary>
/// 设备统计DTO
/// </summary>
public class EquipmentStatisticsDto
{
    public int TotalCount { get; set; }
    public int NormalCount { get; set; }
    public int RepairCount { get; set; }
    public int StoppedCount { get; set; }
    public int ProductionCount { get; set; }
    public int TestingCount { get; set; }
    public int AuxiliaryCount { get; set; }
}
