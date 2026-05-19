namespace Lzq.WorkOrder.Domain.Enums;

/// <summary>
/// 工单状态枚举
/// </summary>
public enum WorkOrderStatusEnum
{
    /// <summary>草稿</summary>
    Draft = 0,
    /// <summary>已派发</summary>
    Dispatched = 1,
    /// <summary>生产中</summary>
    InProgress = 2,
    /// <summary>已完成</summary>
    Completed = 3,
    /// <summary>已关闭</summary>
    Closed = 4,
    /// <summary>已暂停</summary>
    Paused = 5,
    /// <summary>已取消</summary>
    Cancelled = 6,
}
