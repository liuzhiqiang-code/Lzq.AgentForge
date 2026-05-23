using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace Lzq.AgentForge.WebApi.Extensions;

/// <summary>
/// 健康检查扩展
/// </summary>
public static class HealthCheckExtensions
{
    public static void AddLzqHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("MES 服务运行正常"))
            .AddCheck<MemoryHealthCheck>("内存检查");
    }

    public static void MapLzqHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        }).AllowAnonymous();
    }
}

public class MemoryHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken)
    {
        var memoryUsage = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);
        return Task.FromResult(memoryUsage > 1024
            ? HealthCheckResult.Unhealthy($"内存使用率过高: {memoryUsage}MB")
            : HealthCheckResult.Healthy($"内存使用率正常: {memoryUsage}MB"));
    }
}

// 简单的 UI 响应写入器（避免引入额外 NuGet 包）
internal static class UIResponseWriter
{
    public static Task WriteHealthCheckUIResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            entries = report.Entries.Select(e => new
            {
                key = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.ToString()
            })
        };
        return context.Response.WriteAsJsonAsync(result);
    }
}
