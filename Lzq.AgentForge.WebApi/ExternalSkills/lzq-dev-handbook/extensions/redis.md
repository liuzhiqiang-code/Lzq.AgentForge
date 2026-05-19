# 02 - Redis 缓存与分布式锁参考

> 来源: lzq-extensions-redis | 状态: ✅ 已验证

## 配置说明

```json
{
  "Redis": {
    "Prefix": "Lzq:",
    "ConnectionString": "127.0.0.1:6379",
    "IsCluster": false,
    "Sentinels": []
  }
}
```

- `Prefix`：所有 Key 自动加前缀，避免多服务冲突
- `IsCluster`：集群模式使用 Hash Tag `{}` 保证相关 Key 落在同一 Slot

## GetOrSetAsync 机制

1. **防穿透**：空值标记为 `NQ__`，直接返回 null 不查库
2. **防击穿**：分布式锁保证只有一个请求回源
3. **防雪崩**：过期时间叠加随机 0~300 秒

## 锁重试策略

竞争锁失败时：指数退避重试 3 次（100ms / 200ms / 300ms）

## 序列化

- 全局使用 `System.Text.Json`
- 自定义对象必须支持 JSON 序列化
- 空值缓存固定 60 秒
