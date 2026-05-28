namespace Lzq.MES.Application.Contracts.WorkOrder.Dto;

/// <summary>
/// 工单统计结果DTO（供Dashboard模块使用）
/// </summary>
public class WorkOrderStatisticsResultDto
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

    /// <summary>暂停数量</summary>
    public int PausedCount { get; set; }

    /// <summary>取消数量</summary>
    public int CancelledCount { get; set; }
}
