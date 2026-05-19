# 11 - AgentForge WebApi 完整参考架构

> 来源: lzq-agentforge-webapi | 状态: ✅ 已验证

## 完整 Program.cs 注册顺序

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Serilog 日志
builder.AddLzqSerilog(options => {
    options.EnableFile = true;
    options.FilePath = "Logs/myapp-.log";
});

// 2. 核心程序集 + Mapster + 自动注入
builder.Services.AddCoreAssembly().AddMapster().AddCoreAutoInject();

// 3. 健康检查
builder.AddLzqHealthChecks();

// 4. NSwag Swagger
builder.Services.AddLzqNSwag(options => { ... });

// 5. CORS
builder.Services.AddCors(options => { ... });

// 6. 外部 HTTP API
builder.Services.AddExternalHttpApis(builder.Configuration);

// 7. JSON 序列化 (long → string)
builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.Converters.Add(new LongToStringConverter());
});

// 8. JWT 认证
builder.Services.AddLzqJwt(builder.Configuration, options => { ... });

// 9. AI 能力
builder.Services.AddLzqAI().AddSqlSugarChatHistoryProvider().AddLzqAgentSkills();

// 10. SqlSugar 多数据库
builder.Services.AddLzqSqlSugar(builder.Configuration);

// 11. 事件总线
builder.Services.AddEventBus().AddIntegrationEvent(option => {
    option.UseMemoryOutbox();
    option.AddRabbitMqPublisher(opt => { ... });
});

// 12. Minimal APIs (必须最后)
builder.Services.AddCoreMinimalAPIs();

var app = builder.Build();

// 中间件管道
app.UseCors("AllowAll");
app.UseCoreExceptionHandler();
app.UseLzqNSwag();
app.UseAuthentication();
app.UseAuthorization();
app.MapMasaMinimalAPIs();

app.Run();
```

## 必需的中间件顺序

```
CORS → ExceptionHandler → NSwag → Authentication → Authorization → MinimalAPIs
```

## 常见组件

| 组件 | 注册位置 |
|---|---|
| LongToStringConverter | `ConfigureHttpJsonOptions` |
| MemoryHealthCheck | `AddHealthChecks` |
| Swagger 密码保护 | `AddLzqNSwag` options |
