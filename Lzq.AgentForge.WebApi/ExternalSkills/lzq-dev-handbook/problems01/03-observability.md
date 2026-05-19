# 03 — 可观测性与运维

> **关联**: [README.md](./README.md) | **优先级覆盖**: P0-4, P1-5, P2-2

---

## 1. OpenTelemetry 分布式追踪 (P0-4 🔴)

### 当前问题

Lzq 框架仅集成了 Serilog 结构化日志，缺少：
- 分布式追踪（TraceId 在日志中有，但无法跨服务关联）
- 指标导出（Prometheus / OpenTelemetry Metrics）
- 调用链可视化

### 改进方案

```csharp
// Lzq.Extensions.Telemetry — 新增扩展包
// Program.cs
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

builder.Services.AddLzqTelemetry(options =>
{
    options.ServiceName = "Lzq.MES";
    options.ServiceVersion = "1.0.0";

    // 追踪
    options.AddTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter(otlp => otlp.Endpoint = new Uri("http://jaeger:4317")));

    // 指标
    options.AddMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());
});

// 自动注入 TraceId 到 Serilog
builder.AddLzqSerilog();  // 自动关联 OpenTelemetry TraceId
```

### 关键指标建议

```csharp
// 自定义业务指标
public class OrderMetrics
{
    private readonly Counter<long> _orderCreatedCounter;

    public OrderMetrics(MeterProvider meterProvider)
    {
        var meter = new Meter("Lzq.MES.Orders");
        _orderCreatedCounter = meter.CreateCounter<long>("orders.created");
    }

    public void RecordOrderCreated() => _orderCreatedCounter.Add(1);
}
```

### 仪表盘集成

建议提供 Grafana Dashboard JSON 模板，包含：
- API 请求延迟 (p50/p90/p99)
- 请求速率 (RPS)
- 错误率 (4xx/5xx)
- 数据库查询耗时
- 外部 API 调用耗时
- 消息队列积压

---

## 2. 配置校验 — 启动时 Fail-Fast (P1-5 🟡)

### 当前问题

各扩展库的配置独立读取，错误配置只在运行时才发现。应该在启动时集中校验所有必需配置。

### 改进方案

```csharp
// Lzq.Core/Configuration/ConfigurationValidator.cs
public interface IConfigurationValidator
{
    IEnumerable<string> Validate();
}

public class SqlSugarConfigurationValidator : IConfigurationValidator
{
    private readonly IConfiguration _config;

    public IEnumerable<string> Validate()
    {
        var dbConfigs = _config.GetSection("DBConfigs").Get<List<DbConfig>>();
        if (dbConfigs is null || dbConfigs.Count == 0)
            yield return "DBConfigs 未配置任何数据库连接";

        foreach (var db in dbConfigs ?? Enumerable.Empty<DbConfig>())
        {
            if (string.IsNullOrEmpty(db.ConnectionString))
                yield return $"数据库 [{db.Tag}] 缺少 ConnectionString";
            if (string.IsNullOrEmpty(db.Tag))
                yield return "数据库缺少 Tag 标识";
        }
    }
}

// Program.cs — 启动时校验
var validators = app.Services.GetServices<IConfigurationValidator>();
var errors = validators.SelectMany(v => v.Validate()).ToList();
if (errors.Any())
{
    foreach (var error in errors)
        Console.Error.WriteLine($"❌ 配置错误: {error}");
    throw new InvalidOperationException("启动失败：存在配置错误");
}
```

### 标准健康检查增强

```csharp
// 模块级健康检查
public class DatabaseHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken ct)
    {
        try
        {
            await _db.Queryable<object>("SELECT 1").ToListAsync();
            return HealthCheckResult.Healthy("数据库连接正常",
                new Dictionary<string, object>
                {
                    ["latency_ms"] = sw.ElapsedMilliseconds,
                    ["database"] = _config.Tag
                });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("数据库连接失败", ex);
        }
    }
}

// 统一健康检查端点
app.MapLzqHealthChecks("/health", options =>
{
    options.ResponseWriter = async (context, report) =>
    {
        var result = new
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration.TotalMilliseconds,
            Checks = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                e.Value.Duration,
                e.Value.Description,
                e.Value.Data
            })
        };
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(result);
    };
});
```

### 优雅关闭

```csharp
// Program.cs
var app = builder.Build();

// 注册优雅关闭处理
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("正在优雅关闭...");
    // 停止接收新请求
    // 等待进行中的请求完成（已有 30s 超时）
    // 释放数据库连接池
    // 关闭 RabbitMQ 连接
});
```

---

## 3. 后台任务处理集成 (P2-2 🔵)

### 当前问题

框架缺少内置的后台任务调度能力。定时任务、异步批处理需要外部引入 Hangfire 或 Quartz。

### 改进方案

```csharp
// Lzq.Extensions.BackgroundJobs — 建议新增扩展包
builder.Services.AddLzqBackgroundJobs(options =>
{
    options.UseInMemoryStorage(); // 开发环境
    // options.UseSqlServerStorage(connectionString); // 生产环境
});

// 定义后台任务
public class DailyReportJob : IBackgroundJob
{
    public string Name => "每日报表生成";
    public string Cron => "0 6 * * *"; // 每天早上6点

    public async Task ExecuteAsync(IServiceProvider services, CancellationToken ct)
    {
        var reportService = services.GetRequiredService<IReportService>();
        await reportService.GenerateDailyReportAsync(DateTime.Today, ct);
    }
}

// 手动触发
app.Services.GetRequiredService<IBackgroundJobManager>()
    .EnqueueAsync<DailyReportJob>();
```

---

## 4. 可观测性成熟度路线图

```
阶段 1 (当前)         阶段 2 (6 个月)        阶段 3 (12 个月)
─────────────────    ─────────────────    ─────────────────
Serilog 日志         + OpenTelemetry      + 统一仪表盘
请求日志中间件        + Prometheus 指标     + SLO 监控
健康检查端点          + Jaeger 追踪         + 告警规则
                     + 配置 Fail-Fast     + 错误预算追踪
                     + 后台任务           + 容量规划报表
```

---

> 📂 返回 [评估总览](./README.md) | 上一篇：[02-模块架构改进](./02-module-architecture.md)
