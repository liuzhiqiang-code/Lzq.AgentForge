using Lzq.Equipment.Domain.Enums;

namespace Lzq.Equipment.Application.Contracts.Dtos;

/// <summary>
/// 报修单视图DTO
/// </summary>
public class RepairOrderViewDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public long EquipmentId { get; set; }
    public string? EquipmentCode { get; set; }
    public string? EquipmentName { get; set; }
    public int RepairType { get; set; }
    public string? RepairTypeName { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Images { get; set; }
    public long? ReporterId { get; set; }
    public string? ReporterName { get; set; }
    public DateTime? ReportTime { get; set; }
    public RepairPriorityEnum Priority { get; set; }
    public string? PriorityName { get; set; }
    public RepairStatusEnum Status { get; set; }
    public string? StatusName { get; set; }
    public long? RepairUserId { get; set; }
    public string? RepairUserName { get; set; }
    public DateTime? RepairStartTime { get; set; }
    public DateTime? RepairEndTime { get; set; }
    public string? FaultReason { get; set; }
    public string? RepairProcess { get; set; }
    public string? PartsUsed { get; set; }
    public decimal WorkHours { get; set; }
    public decimal Cost { get; set; }
    public long? AcceptorId { get; set; }
    public string? AcceptorName { get; set; }
    public DateTime? AcceptTime { get; set; }
    public string? AcceptComment { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
}

/// <summary>
/// 保养计划视图DTO
/// </summary>
public class MaintenancePlanViewDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long EquipmentId { get; set; }
    public string? EquipmentCode { get; set; }
    public string? EquipmentName { get; set; }
    public MaintenanceTypeEnum MaintenanceType { get; set; }
    public string? MaintenanceTypeName { get; set; }
    public int CycleDays { get; set; }
    public DateTime PlanDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public MaintenancePlanStatusEnum Status { get; set; }
    public string? StatusName { get; set; }
    public long? ResponsibleId { get; set; }
    public string? ResponsibleName { get; set; }
    public string? Content { get; set; }
    public string? Standard { get; set; }
    public int DurationMinutes { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
}

/// <summary>
/// 保养记录视图DTO
/// </summary>
public class MaintenanceRecordViewDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public long? PlanId { get; set; }
    public long EquipmentId { get; set; }
    public string? EquipmentCode { get; set; }
    public string? EquipmentName { get; set; }
    public MaintenanceTypeEnum MaintenanceType { get; set; }
    public string? MaintenanceTypeName { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public long? MaintainerId { get; set; }
    public string? MaintainerName { get; set; }
    public int DurationMinutes { get; set; }
    public string? Content { get; set; }
    public string? Result { get; set; }
    public string? Images { get; set; }
    public string? ProblemsFound { get; set; }
    public DateTime? NextReminderDate { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
}

/// <summary>
/// 维修统计DTO
/// </summary>
public class RepairStatisticsDto
{
    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public decimal TotalWorkHours { get; set; }
    public decimal TotalCost { get; set; }
    public int UrgentCount { get; set; }
}
