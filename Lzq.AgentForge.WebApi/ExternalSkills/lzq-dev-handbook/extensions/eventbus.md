# 06 - EventBus CQRS 参考

> 来源: lzq-extensions-eventbus | 状态: ✅ 已验证

## 核心接口

| 抽象 | 说明 |
|---|---|
| `ICommand<TResponse>` | 有返回值命令 |
| `ICommand` | 无返回值命令 |
| `IQuery<TResponse>` | 查询 |
| `ILocalEvent` | 进程内事件（多播） |
| `IIntegrationEvent` | 集成事件 |

## 管道行为顺序

```
LoggingBehavior → ValidatorBehavior → TransactionBehavior → IntegrationEventMiddleware
```

## Outbox 模式

- 未注册 Outbox：直接从容器获取 `IIntegrationPublisher` 发布
- 注册 Outbox：事件保存到 `IIntegrationEventStore`，后台任务发布

## 事务与事件协同

`[UnitOfWork]` 先于 `IntegrationEventMiddleware` 执行 → 事务提交后事件才被拦截

## 注意事项

- `IntegrationEventMiddleware` 直接返回 `default`，`SendAsync` 返回值被忽略
- 建议：发送集成事件用 `PublishAsync` 而非 `SendAsync`
