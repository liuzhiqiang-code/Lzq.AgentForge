namespace Lzq.MES.Domain.Enums;

/// <summary>
/// 质检单状态枚�?/// </summary>
public enum QCOrderStatusEnum
{
    /// <summary>待检�?/summary>
    Pending = 0,
    /// <summary>检验中</summary>
    InProgress = 1,
    /// <summary>合格</summary>
    Qualified = 2,
    /// <summary>不合�?/summary>
    Unqualified = 3,
    /// <summary>已处�?/summary>
    Processed = 4,
    /// <summary>已取�?/summary>
    Cancelled = 5,
}

/// <summary>
/// 质检类型枚举
/// </summary>
public enum QCTypeEnum
{
    /// <summary>来料检�?(IQC)</summary>
    IQC = 1,
    /// <summary>过程检�?(PQC)</summary>
    PQC = 2,
    /// <summary>出货检�?(OQC)</summary>
    OQC = 3,
}

/// <summary>
/// 检验结果枚�?/// </summary>
public enum QCResultEnum
{
    /// <summary>合格</summary>
    Pass = 1,
    /// <summary>不合�?/summary>
    Fail = 2,
    /// <summary>让步接收</summary>
    AcceptWithRestriction = 3,
}

/// <summary>
/// 不良品处理方式枚�?/// </summary>
public enum DefectHandlingEnum
{
    /// <summary>返工</summary>
    Rework = 1,
    /// <summary>报废</summary>
    Scrap = 2,
    /// <summary>降级使用</summary>
    Downgrade = 3,
    /// <summary>退�?/summary>
    Return = 4,
    /// <summary>特采</summary>
    AcceptSpecial = 5,
}

/// <summary>
/// 不良品状态枚�?/// </summary>
public enum DefectStatusEnum
{
    /// <summary>待处�?/summary>
    Pending = 0,
    /// <summary>处理�?/summary>
    Processing = 1,
    /// <summary>已处�?/summary>
    Processed = 2,
}
