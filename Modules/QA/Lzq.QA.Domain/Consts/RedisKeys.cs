namespace Lzq.QA.Domain.Consts;

/// <summary>
/// QA 模块 Redis Key 常量定义
/// </summary>
public class RedisKeys
{
    /// <summary>
    /// 质检单详情缓存，TTL 30分钟
    /// 格式化参数：id
    /// </summary>
    public const string Get = "QCOrder:Get:{0}";

    /// <summary>
    /// 质检单检验明细缓存，TTL 30分钟
    /// 格式化参数：qcOrderId
    /// </summary>
    public const string Items = "QCOrder:Items:{0}";
}
