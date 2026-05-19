# 01 - Serilog 结构化日志参考

> 来源: lzq-extensions-serilog | 状态: ✅ 已验证

## 输出模板

```
{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [TraceId:{TraceId}] {Message:lj}{NewLine}{Exception}
```

## 自动附加属性

| 属性 | 来源 | 说明 |
|---|---|---|
| `TraceId` | `Activity.Current.TraceId` | 分布式追踪 ID |
| `Method` | `IHttpContextAccessor` | HTTP 请求方法 |
| `Path` | `IHttpContextAccessor` | HTTP 请求路径 |

## 日志级别覆写规则

| 命名空间 | 最低级别 | 原因 |
|---|---|---|
| `Microsoft` | `Information` | 减少框架噪音 |
| `Microsoft.AspNetCore` | `Warning` | 减少框架噪音 |
| 文件 Sink | `Debug` | 确保所有日志落入文件 |

## Sink 异步支持

| Sink | 支持异步 | 说明 |
|---|---|---|
| Console | ✅ | `ConsoleAsync` 默认 true |
| File | ✅ | `FileAsync` 默认 true |
| SQLite | ❌ | 同步写入 |
| Loki | ❌ | 同步推送 |

## HttpLoggingMiddleware

- 仅记录 `/api` 路径下的请求
- 正常：`HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms`
- 异常：`HTTP {Method} {Path} failed after {ElapsedMs}ms`（异常会重新抛出）

## OutputTemplate 与 JSON 格式

- `OutputTemplate` 为 `null` 时：控制台使用 `CompactJsonFormatter`，文件使用自身默认模板
- 自定义时：同时应用于控制台和文件
