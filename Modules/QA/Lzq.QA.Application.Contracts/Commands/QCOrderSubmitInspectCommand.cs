using Lzq.QA.Domain.Enums;

namespace Lzq.QA.Application.Contracts.Commands;

/// <summary>
/// 提交检验明细命令
/// </summary>
public record QCOrderSubmitInspectCommand
{
    /// <summary>质检单ID</summary>
    public long Id { get; init; }

    /// <summary>检验明细列表</summary>
    public List<QCOrderItemInspectCommand> Items { get; init; } = new();
}

/// <summary>
/// 检验明细命令
/// </summary>
public record QCOrderItemInspectCommand
{
    /// <summary>检验项目名称</summary>
    public string ItemName { get; init; } = string.Empty;

    /// <summary>检验项目类型</summary>
    public int ItemType { get; init; }

    /// <summary>检验标准/要求</summary>
    public string? Standard { get; init; }

    /// <summary>检验方法</summary>
    public string? Method { get; init; }

    /// <summary>抽样数量</summary>
    public int SampleQty { get; init; }

    /// <summary>合格数量</summary>
    public int QualifiedQty { get; init; }

    /// <summary>不合格数量</summary>
    public int UnqualifiedQty { get; init; }

    /// <summary>检验结果：1-合格 2-不合格 3-让步接收</summary>
    public QCResultEnum Result { get; init; }

    /// <summary>不合格描述</summary>
    public string? DefectDesc { get; init; }

    /// <summary>不合格代码</summary>
    public string? DefectCode { get; init; }

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}
