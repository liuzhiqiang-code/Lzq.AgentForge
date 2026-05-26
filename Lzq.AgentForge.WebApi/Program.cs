using Lzq.AgentForge.WebApi;
using Lzq.Core.Modules;
using Lzq.Extensions.Serilog;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. Serilog 结构化日志（文件输出）
// ============================================================
builder.AddLzqSerilog(options =>
{
    options.EnableFile = true;
    options.FilePath = "Logs/mes-.log";
});

// 模块依赖通过 [DependsOn] 属性自动解析，按拓扑排序加载
await builder.AddApplicationAsync<WebApiModule>();

var app = builder.Build();
await app.InitializeApplicationAsync();

app.Run();
