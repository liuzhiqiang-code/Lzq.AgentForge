using Lzq.AgentForge.WebApi.Extensions;
using Lzq.Core;
using Lzq.Core.Json;
using Lzq.Extensions.AI;
using Lzq.Extensions.EventBus;
using Lzq.Extensions.EventBus.RabbitMq;
using Lzq.Extensions.ExternalHttpApi;
using Lzq.Extensions.Jwt;
using Lzq.Extensions.NSwag;
using Lzq.Extensions.Redis;
using Lzq.Extensions.Serilog;
using Lzq.Extensions.SqlSugar;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. Serilog 结构化日志（文件输出）
// ============================================================
builder.AddLzqSerilog(options =>
{
    options.EnableFile = true;
    options.FilePath = "Logs/mes-.log";
});

// ============================================================
// 2. 加载核心程序集 + Mapster 映射 + 自动依赖注入
//    AddCoreAssembly("Lzq.MES") 会扫描所有 Lzq.MES.* 程序集
// ============================================================
builder.Services.AddCoreAssembly().AddMapster().AddCoreAutoInject();

// ============================================================
// 3. 健康检查
// ============================================================
builder.AddLzqHealthChecks();

// ============================================================
// 4. CORS（开发环境允许所有来源）
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddExternalHttpApis(builder.Configuration);

// ============================================================
// 5. JSON 序列化：long → string（防止 JS 精度丢失）
// ============================================================
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new LongToStringConverter());
    options.SerializerOptions.Converters.Add(new LongNullableToStringConverter());
    options.SerializerOptions.Converters.Add(new DateTimeFormatConverter());
    options.SerializerOptions.Converters.Add(new DateTimeNullableFormatConverter());
});

// ============================================================
// 6. SqlSugar 多数据库配置
// ============================================================
builder.Services.AddLzqSqlSugar(builder.Configuration);

// ============================================================
// 6.1 Redis 缓存配置
// ============================================================
builder.Services.AddLzqRedis(builder.Configuration);

// ============================================================
// 7. JWT 认证
// ============================================================
builder.Services.AddLzqJwt(builder.Configuration, options =>
{
    options.Issuer = "Lzq.MES";
    options.Audience = "Lzq.MES";
    options.SecurityKey = "lzq-mes-secret-key-min-16-chars!";
});

builder.Services.AddLzqAI()
    .AddSqlSugarChatHistoryProvider();

// ============================================================
// 8. NSwag (Swagger) 文档 + UI 密码保护
// ============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddLzqNSwag(options =>
{
    options.Documents =
    [
        new SwaggerDocumentInfo { Name = "MES", Title = "Lzq.MES API" }
    ];
    options.Title = "Lzq.MES - 小型企业 MES 系统";
    options.Version = "1.0.0";
    options.EnableSwaggerUI = !builder.Environment.IsProduction();
    options.EnableSwaggerUIPassword = true;
    options.SwaggerUIPassword = "mes123";
    options.SwaggerUIPasswordCookieExpirationMinutes = 720;
});

// ============================================================
// 9. 事件总线（集成事件 + RabbitMQ + 内存 Outbox）
// ============================================================
builder.Services.AddEventBus()
    .AddIntegrationEvent(option =>
    {
        option.UseMemoryOutbox();
        option.AddRabbitMqPublisher(opt =>
        {
            // RabbitMQ 连接信息从 appsettings.json 读取
            // 此处为兜底默认值
        });
    });

// ============================================================
// 10. Minimal APIs 注册（必须在 Build 前最后调用）
// ============================================================
builder.Services.AddCoreMinimalAPIs();

var app = builder.Build();

// ============================================================
// 中间件管道
// ============================================================
app.UseCors("AllowAll");
app.UseCoreExceptionHandler();      // 全局异常处理 → ApiResult
app.UseLzqNSwag();                   // Swagger UI
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapLzqHealthChecks();            // /health 端点
app.MapMasaMinimalAPIs();            // ServiceBase 动态 API

app.Run();
