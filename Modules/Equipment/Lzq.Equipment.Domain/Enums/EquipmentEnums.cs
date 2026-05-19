namespace Lzq.Equipment.Domain.Enums;

/// <summary>
/// 设备状态枚举
/// </summary>
public enum EquipmentStatusEnum
{
    /// <summary>正常</summary>
    Normal = 0,
    /// <summary>维修中</summary>
    UnderRepair = 1,
    /// <summary>保养中</summary>
    UnderMaintenance = 2,
    /// <summary>停机</summary>
    Stopped = 3,
    /// <summary>报废</summary>
    Scrapped = 4,
}

/// <summary>
/// 设备类型枚举
/// </summary>
public enum EquipmentTypeEnum
{
    /// <summary>生产设备</summary>
    Production = 1,
    /// <summary>检测设备</summary>
    Testing = 2,
    /// <summary>辅助设备</summary>
    Auxiliary = 3,
    /// <summary>动力设备</summary>
    Power = 4,
    /// <summary>运输设备</summary>
    Transport = 5,
}

/// <summary>
/// 点检结果枚举
/// </summary>
public enum InspectionResultEnum
{
    /// <summary>正常</summary>
    Normal = 0,
    /// <summary>异常</summary>
    Abnormal = 1,
    /// <summary>待维修</summary>
    NeedRepair = 2,
}

/// <summary>
/// 维修优先级枚举
/// </summary>
public enum RepairPriorityEnum
{
    /// <summary>紧急</summary>
    Urgent = 1,
    /// <summary>高</summary>
    High = 2,
    /// <summary>中</summary>
    Medium = 3,
    /// <summary>低</summary>
    Low = 4,
}

/// <summary>
/// 维修状态枚举
/// </summary>
public enum RepairStatusEnum
{
    /// <summary>待派工</summary>
    Pending = 0,
    /// <summary>已派工</summary>
    Assigned = 1,
    /// <summary>维修中</summary>
    InProgress = 2,
    /// <summary>已完工待验收</summary>
    Completed = 3,
    /// <summary>已验收</summary>
    Accepted = 4,
    /// <summary>已取消</summary>
    Cancelled = 5,
}

/// <summary>
/// 保养类型枚举
/// </summary>
public enum MaintenanceTypeEnum
{
    /// <summary>日常保养</summary>
    Daily = 1,
    /// <summary>一级保养</summary>
    Level1 = 2,
    /// <summary>二级保养</summary>
    Level2 = 3,
    /// <summary>三级保养</summary>
    Level3 = 4,
    /// <summary>精度保养</summary>
    Precision = 5,
}

/// <summary>
/// 保养计划状态枚举
/// </summary>
public enum MaintenancePlanStatusEnum
{
    /// <summary>待执行</summary>
    Pending = 0,
    /// <summary>执行中</summary>
    InProgress = 1,
    /// <summary>已完成</summary>
    Completed = 2,
    /// <summary>已延期</summary>
    Delayed = 3,
    /// <summary>已取消</summary>
    Cancelled = 4,
}
