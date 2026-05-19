namespace Lzq.BaseData.Domain.Enums;

/// <summary>
/// 通用实体启用状态
/// </summary>
public enum EnableStatusEnum
{
    /// <summary>禁用</summary>
    Disabled = 0,
    /// <summary>启用</summary>
    Enabled = 1,
}

/// <summary>
/// 设备运行状态
/// </summary>
public enum EquipmentStatusEnum
{
    /// <summary>运行中</summary>
    Running = 0,
    /// <summary>停机</summary>
    Stopped = 1,
    /// <summary>维修中</summary>
    Repairing = 2,
    /// <summary>已报废</summary>
    Scrapped = 3,
}

/// <summary>
/// 点检结果
/// </summary>
public enum CheckResultEnum
{
    /// <summary>正常</summary>
    Normal = 0,
    /// <summary>异常</summary>
    Abnormal = 1,
    /// <summary>已处理</summary>
    Resolved = 2,
}
