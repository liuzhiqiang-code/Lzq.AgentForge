namespace Lzq.MES.Application.Contracts.Dtos;

/// <summary>
/// 看板统计基类
/// </summary>
public class KanbanStatBaseDto
{
    /// <summary>统计时间</summary>
    public DateTime StatTime { get; set; }

    /// <summary>产线ID</summary>
    public long? LineId { get; set; }

    /// <summary>产线名称</summary>
    public string? LineName { get; set; }
}

/// <summary>
/// 产量统计DTO
/// </summary>
public class ProductionOutputDto : KanbanStatBaseDto
{
    /// <summary>计划产量</summary>
    public int PlanQuantity { get; set; }

    /// <summary>实际产量</summary>
    public int ActualQuantity { get; set; }

    /// <summary>完成率(%)</summary>
    public decimal CompletionRate { get; set; }
}

/// <summary>
/// 产量统计汇总DTO
/// </summary>
public class ProductionOutputSummaryDto
{
    /// <summary>今日产量</summary>
    public int TodayOutput { get; set; }

    /// <summary>本周产量</summary>
    public int WeekOutput { get; set; }

    /// <summary>本月产量</summary>
    public int MonthOutput { get; set; }

    /// <summary>今日完成率</summary>
    public decimal TodayCompletionRate { get; set; }

    /// <summary>本周完成率</summary>
    public decimal WeekCompletionRate { get; set; }

    /// <summary>本月完成率</summary>
    public decimal MonthCompletionRate { get; set; }

    /// <summary>产量趋势列表</summary>
    public List<DailyOutputDto> TrendList { get; set; } = new();
}

/// <summary>
/// 每日产量DTO
/// </summary>
public class DailyOutputDto
{
    /// <summary>日期</summary>
    public DateTime Date { get; set; }

    /// <summary>产量</summary>
    public int Quantity { get; set; }
}

/// <summary>
/// 不良率趋势DTO
/// </summary>
public class DefectRateTrendDto : KanbanStatBaseDto
{
    /// <summary>抽检数量</summary>
    public int InspectionCount { get; set; }

    /// <summary>合格数量</summary>
    public int QualifiedCount { get; set; }

    /// <summary>不良数量</summary>
    public int DefectCount { get; set; }

    /// <summary>合格率(%)</summary>
    public decimal QualifiedRate { get; set; }

    /// <summary>不良率(%)</summary>
    public decimal DefectRate { get; set; }
}

/// <summary>
/// 不良率汇总DTO
/// </summary>
public class DefectRateSummaryDto
{
    /// <summary>今日合格率</summary>
    public decimal TodayQualifiedRate { get; set; }

    /// <summary>本周合格率</summary>
    public decimal WeekQualifiedRate { get; set; }

    /// <summary>本月合格率</summary>
    public decimal MonthQualifiedRate { get; set; }

    /// <summary>不良率趋势列表</summary>
    public List<DailyDefectRateDto> TrendList { get; set; } = new();
}

/// <summary>
/// 每日不良率DTO
/// </summary>
public class DailyDefectRateDto
{
    /// <summary>日期</summary>
    public DateTime Date { get; set; }

    /// <summary>合格率</summary>
    public decimal QualifiedRate { get; set; }

    /// <summary>不良数量</summary>
    public int DefectCount { get; set; }
}

/// <summary>
/// 工单完成率DTO
/// </summary>
public class WorkOrderCompletionDto
{
    /// <summary>工单总数</summary>
    public int TotalCount { get; set; }

    /// <summary>已完成数量</summary>
    public int CompletedCount { get; set; }

    /// <summary>进行中数量</summary>
    public int InProgressCount { get; set; }

    /// <summary>待开始数量</summary>
    public int PendingCount { get; set; }

    /// <summary>完成率(%)</summary>
    public decimal CompletionRate { get; set; }
}

/// <summary>
/// 工单完成率汇总DTO
/// </summary>
public class WorkOrderCompletionSummaryDto
{
    /// <summary>今日完成率</summary>
    public decimal TodayCompletionRate { get; set; }

    /// <summary>本周完成率</summary>
    public decimal WeekCompletionRate { get; set; }

    /// <summary>本月完成率</summary>
    public decimal MonthCompletionRate { get; set; }

    /// <summary>今日工单统计</summary>
    public WorkOrderCompletionDto TodayStats { get; set; } = new();

    /// <summary>本周工单统计</summary>
    public WorkOrderCompletionDto WeekStats { get; set; } = new();

    /// <summary>本月工单统计</summary>
    public WorkOrderCompletionDto MonthStats { get; set; } = new();
}

/// <summary>
/// 设备状态概览DTO
/// </summary>
public class EquipmentStatusOverviewDto
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

    /// <summary>设备状态列表</summary>
    public List<EquipmentStatusItemDto> StatusList { get; set; } = new();
}

/// <summary>
/// 设备状态项DTO
/// </summary>
public class EquipmentStatusItemDto
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

    /// <summary>正常率</summary>
    public decimal NormalRate { get; set; }
}

/// <summary>
/// 看板配置DTO
/// </summary>
public class DashboardConfigDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ConfigType { get; set; }
    public int RefreshInterval { get; set; }
    public int CacheTtl { get; set; }
    public string? ConfigJson { get; set; }
    public bool IsEnabled { get; set; }
    public string? Remark { get; set; }
}
