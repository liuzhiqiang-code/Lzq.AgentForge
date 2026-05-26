using Lzq.AgentForge.WebApi.Extensions;
using Lzq.AI.Application;
using Lzq.BaseData.Application;
using Lzq.Core.Json;
using Lzq.Core.Modules;
using Lzq.Dashboard.Application;
using Lzq.Equipment.Application;
using Lzq.Extensions.EventBus;
using Lzq.Extensions.ExternalHttpApi;
using Lzq.Extensions.Jwt;
using Lzq.Extensions.NSwag;
using Lzq.Extensions.Redis;
using Lzq.QA.Application;
using Lzq.Rbac.Application;
using Lzq.Temp.Application;
using Lzq.WorkOrder.Application;
using Masa.BuildingBlocks.Data;

namespace Lzq.AgentForge.WebApi;

[DependsOn(
    typeof(NSwagModule),
    typeof(ExternalHttpApiModule),
    typeof(JwtModule),
    typeof(RedisModule),
    typeof(EventBusModule),
    typeof(RbacApplicationModule),
    typeof(AIApplicationModule),
    typeof(BaseDataApplicationModule),
    typeof(EquipmentApplicationModule),
    typeof(QAApplicationModule),
    typeof(WorkOrderApplicationModule),
    typeof(DashboardApplicationModule),
    typeof(TempApplicationModule)
)]
public class WebApiModule : BaseModule
{
    public override void PreConfigureServices(ModuleServiceContext context)
    {
        var services = context.Services;
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
                options.DisableTrimMethodPrefix = true;
                options.MapHttpMethodsForUnmatched = new string[] { "Post" };
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
