using Lzq.QA.Domain.Enums;

namespace Lzq.QA.Application.Contracts.Commands;

/// <summary>
/// 创建不良品记录命令
/// </summary>
public record DefectRecordCreateCommand
{
    /// <summary>质检单ID</summary>
    public long? QCOrderId { get; init; }

    /// <summary>质检单编号</summary>
    public string? QCOrderCode { get; init; }

    /// <summary>关联工单ID</summary>
    public long? WorkOrderId { get; init; }

    /// <summary>关联工单编号</summary>
    public string? WorkOrderCode { get; init; }

    /// <summary>产品ID</summary>
    public long? ProductId { get; init; }

    /// <summary>产品名称</summary>
    public string? ProductName { get; init; }

    /// <summary>产品规格/型号</summary>
    public string? ProductSpec { get; init; }

    /// <summary>批号/批次</summary>
    public string? BatchNo { get; init; }

    /// <summary>不良品数量</summary>
    public int DefectQty { get; init; }

    /// <summary>不合格代码/类型</summary>
    public string DefectCode { get; init; } = string.Empty;

    /// <summary>不合格描述</summary>
    public string DefectDesc { get; init; } = string.Empty;

    /// <summary>不良品图片（JSON数组）</summary>
    public string? DefectImages { get; init; }

    /// <summary>不良品状态</summary>
    public DefectStatusEnum Status { get; init; } = DefectStatusEnum.Pending;

    /// <summary>是否需要评审</summary>
    public bool NeedReview { get; init; } = false;

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}

/// <summary>
/// 处理不良品命令
/// </summary>
public record DefectRecordHandleCommand
{
    /// <summary>不良品记录ID</summary>
    public long Id { get; init; }

    /// <summary>处理方式</summary>
    public DefectHandlingEnum HandlingType { get; init; }

    /// <summary>处理说明</summary>
    public string? HandlingRemark { get; init; }

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}

/// <summary>
/// 评审不良品命令
/// </summary>
public record DefectRecordReviewCommand
{
    /// <summary>不良品记录ID</summary>
    public long Id { get; init; }

    /// <summary>评审结果</summary>
    public string ReviewResult { get; init; } = string.Empty;

    /// <summary>处理方式</summary>
    public DefectHandlingEnum? HandlingType { get; init; }

    /// <summary>评审说明</summary>
    public string? ReviewRemark { get; init; }
}
