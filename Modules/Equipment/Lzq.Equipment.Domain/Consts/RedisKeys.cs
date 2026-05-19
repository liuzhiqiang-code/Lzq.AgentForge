namespace Lzq.Equipment.Domain.Consts;

/// <summary>
/// Equipment 模块 Redis Key 常量定义
/// </summary>
public class RedisKeys
{
    /// <summary>
    /// 设备详情缓存，TTL 1小时
    /// 格式化参数：id
    /// </summary>
    public const string Get = "Equipment:Get:{0}";

    /// <summary>
    /// 设备统计缓存，TTL 10分钟
    /// </summary>
    public const string Statistics = "Equipment:Statistics";
}
