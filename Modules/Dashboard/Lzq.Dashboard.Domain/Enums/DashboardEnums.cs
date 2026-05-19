namespace Lzq.Dashboard.Domain.Enums;

/// <summary>
/// 统计周期枚举
/// </summary>
public enum StatisticPeriodEnum
{
    /// <summary>今日</summary>
    Today = 1,
    /// <summary>本周</summary>
    ThisWeek = 2,
    /// <summary>本月</summary>
    ThisMonth = 3,
}

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
}
