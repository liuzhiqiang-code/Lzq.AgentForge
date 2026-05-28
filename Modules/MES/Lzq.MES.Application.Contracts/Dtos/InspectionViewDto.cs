using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Dtos;

/// <summary>
/// 点检计划视图DTO
/// </summary>
public class InspectionPlanViewDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long EquipmentId { get; set; }
    public string? EquipmentCode { get; set; }
    public string? EquipmentName { get; set; }
    public int CycleType { get; set; }
    public string? CycleTypeName { get; set; }
    public int CycleValue { get; set; }
    public DateTime? NextInspectDate { get; set; }
    public int ItemCount { get; set; }
    public long? ExecutorId { get; set; }
    public string? ExecutorName { get; set; }
    public bool IsEnabled { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}

/// <summary>
/// 点检记录视图DTO
/// </summary>
public class InspectionRecordViewDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public long PlanId { get; set; }
    public long EquipmentId { get; set; }
    public string? EquipmentCode { get; set; }
    public string? EquipmentName { get; set; }
    public DateTime InspectDate { get; set; }
    public InspectionResultEnum Result { get; set; }
    public string? ResultName { get; set; }
    public long? InspectorId { get; set; }
    public string? InspectorName { get; set; }
    public DateTime? CompletedTime { get; set; }
    public int DurationMinutes { get; set; }
    public string? AbnormalDesc { get; set; }
    public bool CreateRepairOrder { get; set; }
    public long? RepairOrderId { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
}

/// <summary>
/// 点检明细视图DTO
/// </summary>
public class InspectionItemViewDto
{
    public long Id { get; set; }
    public long PlanId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Standard { get; set; }
    public string? Method { get; set; }
    public int ItemType { get; set; }
    public string? ItemTypeName { get; set; }
    public bool IsRequired { get; set; }
    public int Sort { get; set; }
    public string? Remark { get; set; }
}
