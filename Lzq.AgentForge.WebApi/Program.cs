using Lzq.AgentForge.WebApi;
using Lzq.AI.Application;
using Lzq.BaseData.Application;
using Lzq.Core;
using Lzq.Core.Modules;
using Lzq.Dashboard.Application;
using Lzq.Equipment.Application;
using Lzq.Extensions.AI;
using Lzq.Extensions.EventBus;
using Lzq.Extensions.ExternalHttpApi;
using Lzq.Extensions.Jwt;
using Lzq.Extensions.NSwag;
using Lzq.Extensions.Redis;
using Lzq.Extensions.Serilog;
using Lzq.Extensions.SqlSugar;
using Lzq.QA.Application;
using Lzq.Rbac.Application;
using Lzq.Temp.Application;
using Lzq.WorkOrder.Application;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. Serilog 结构化日志（文件输出）
// ============================================================
builder.AddLzqSerilog(options =>
{
    options.EnableFile = true;
    options.FilePath = "Logs/mes-.log";
});


builder.SerializationModules()
    // 模块注册顺序：核心模块放在最前面，WebApi模块放在最后面，业务模块根据需要放在中间。
    .AddModule<CoreModule>()
    .AddModule<NSwagModule>()
    .AddModule<ExternalHttpApiModule>()
    .AddModule<JwtModule>()
    .AddModule<SqlSugarModule>()
    .AddModule<RedisModule>()
    .AddModule<EventBusModule>()
    .AddModule<AIModule>()

    // 业务模块
    .AddModule<RbacApplicationModule>()
    .AddModule<AIApplicationModule>()
    .AddModule<BaseDataApplicationModule>()
    .AddModule<EquipmentApplicationModule>()
    .AddModule<QAApplicationModule>()
    .AddModule<WorkOrderApplicationModule>()
    .AddModule<DashboardApplicationModule>()
    .AddModule<TempApplicationModule>()

    .AddModule<WebApiModule>()
    .ConfigureModules();

var app = builder.Build();
app.UseSerializationModules();

app.Run();
