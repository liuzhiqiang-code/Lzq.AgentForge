namespace Lzq.MES.Application.Contracts.Dtos;

/// <summary>
/// 设备状态概览结果DTO（供Dashboard模块使用）
/// </summary>
public class EquipmentStatusOverviewResultDto
{
    /// <summary>设备总数</summary>
    public int TotalCount { get; set; }

    /// <summary>正常数量</summary>
    public int NormalCount { get; set; }

    /// <summary>维修中数量</summary>
    public int UnderRepairCount { get; set; }

    /// <summary>保养中数量</summary>
    public int UnderMaintenanceCount { get; set; }

    /// <summary>停机数量</summary>
    public int StoppedCount { get; set; }

    /// <summary>正常率(%)</summary>
    public decimal NormalRate { get; set; }
}

/// <summary>
/// 按产线分组的设备状态结果DTO（供Dashboard模块使用）
/// </summary>
public class EquipmentStatusByLineResultDto
{
    /// <summary>产线ID</summary>
    public long? LineId { get; set; }

    /// <summary>产线名称</summary>
    public string? LineName { get; set; }

    /// <summary>设备总数</summary>
    public int TotalCount { get; set; }

    /// <summary>正常数量</summary>
    public int NormalCount { get; set; }

    /// <summary>异常数量</summary>
    public int AbnormalCount { get; set; }

    /// <summary>正常率(%)</summary>
    public decimal NormalRate { get; set; }
}
