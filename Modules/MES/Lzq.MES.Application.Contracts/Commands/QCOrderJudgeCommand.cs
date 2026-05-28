using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Commands;

/// <summary>
/// 判定质检单命令
/// </summary>
public record QCOrderJudgeCommand
{
    /// <summary>质检单ID</summary>
    public long Id { get; init; }

    /// <summary>判定结果：2-合格 3-不合格</summary>
    public QCOrderStatusEnum Result { get; init; }

    /// <summary>合格数量</summary>
    public int QualifiedQty { get; init; }

    /// <summary>不合格数量</summary>
    public int UnqualifiedQty { get; init; }

    /// <summary>检验结论</summary>
    public string? Conclusion { get; init; }

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}

/// <summary>
/// 取消质检单命令
/// </summary>
public record QCOrderCancelCommand
{
    /// <summary>质检单ID</summary>
    public long Id { get; init; }

    /// <summary>取消原因</summary>
    public string? Reason { get; init; }
}
