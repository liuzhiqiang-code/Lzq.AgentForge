namespace Lzq.Rbac.Domain.Consts;

/// <summary>
/// Rbac 模块 Redis Key 常量定义
/// </summary>
public class RedisKeys
{
    /// <summary>
    /// 用户列表缓存，TTL 1小时
    /// </summary>
    public const string UserList = "User:List";
}
