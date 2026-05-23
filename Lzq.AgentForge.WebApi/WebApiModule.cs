using Lzq.AgentForge.WebApi.Extensions;
using Lzq.Core.Json;
using Lzq.Core.Modules;
using Masa.BuildingBlocks.Data;

namespace Lzq.AgentForge.WebApi;

public class WebApiModule : BaseModule
{
    public override void Configure(ModuleConfigureContext context)
    {
        var currentAssembly = typeof(WebApiModule).Assembly;
        MasaApp.TryAddAssemblies(currentAssembly);
    }

    public override void PreConfigureServices(ModuleServiceContext context)
    {
        var services = context.Services;
        services.AddAutoInject(MasaApp.GetAssemblies());
        services.AddLzqHealthChecks();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new LongToStringConverter());
            options.SerializerOptions.Converters.Add(new LongNullableToStringConverter());
            options.SerializerOptions.Converters.Add(new DateTimeFormatConverter());
            options.SerializerOptions.Converters.Add(new DateTimeNullableFormatConverter());
        });
    }

    public override void PostConfigureServices(ModuleServiceContext context)
    {
        context.Services.AddEndpointsApiExplorer();
        context.Services
            .AddMasaMinimalAPIs(options =>
            {
                options.DisableTrimMethodPrefix = true;//禁用移除方法前缀(上方 `Get`、`Post`、`Put`、`Delete` 请求的前缀)
                options.MapHttpMethodsForUnmatched = new string[] { "Post" };//当前服务禁用自动注册路由
            });
    }

    public override void OnPreApplicationInitialization(ModuleInitContext context)
    {
        var app = context.App;
        app.UseCors("AllowAll");
        app.UseHttpsRedirection();
    }

    public override void OnPostApplicationInitialization(ModuleInitContext context)
    {
        var app = context.App;
        app.MapLzqHealthChecks();
        app.MapMasaMinimalAPIs();
    }
}
