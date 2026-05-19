# 07 - EventBus RabbitMQ 参考

> 来源: lzq-extensions-eventbus-rabbitmq | 状态: ✅ 已验证

## 拓扑结构

| 组件 | 名称规则 |
|---|---|
| 主交换机 | `{topicName}` (类型: x-delayed-message) |
| 主队列 | `{topicName}_queue` |
| 路由键 | `{topicName}_routingKey` |
| 死信交换机 | `{topicName}_dead_letter_exchange` |
| 死信队列 | `{topicName}_dead_letter_queue` |

## 延迟消息依赖

⚠️ 需要 RabbitMQ 安装 `rabbitmq_delayed_message_exchange` 插件

如未安装：
```bash
rabbitmq-plugins enable rabbitmq_delayed_message_exchange
```

## 关键实现细节

- `RabbitMqPublisher` 注册为 **Singleton**
- 每次 `PublishAsync` 新建 `IChannel`
- 连接支持自动恢复 (`AutomaticRecoveryEnabled = true`)
- 队列长度限制 10000 条（硬编码）

## 故障排查

| 问题 | 原因 | 解决 |
|---|---|---|
| `unknown exchange type 'x-delayed-message'` | 未安装插件 | 安装插件或改用 direct |
| 连接拒绝 | 服务未启动 | 检查主机名/端口 |
| 消息未到队列 | 路由键不匹配 | 检查 `{topicName}_routingKey` |
