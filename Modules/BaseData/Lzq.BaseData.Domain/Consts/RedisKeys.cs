namespace Lzq.BaseData.Domain.Consts;

/// <summary>
/// BaseData 模块 Redis Key 常量定义
/// </summary>
public class RedisKeys
{
    /// <summary>
    /// 工厂树缓存（四级结构：工厂→车间→产线→工序），TTL 2小时
    /// </summary>
    public const string FactoryTree = "Factory:Tree";
}
