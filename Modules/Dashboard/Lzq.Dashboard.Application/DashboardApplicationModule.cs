using Lzq.Core.Modules;
using Lzq.Extensions.SqlSugar;
using Masa.BuildingBlocks.Data;
using System.Reflection;

namespace Lzq.Dashboard.Application;

[DependsOn(typeof(SqlSugarModule))]
public class DashboardApplicationModule : BaseModule
{
    public override void Configure(ModuleConfigureContext context)
    {
        Assembly[] assemblies = [
            Assembly.Load("Lzq.Dashboard.Domain"),
            Assembly.Load("Lzq.Dashboard.Application.Contracts"),
            Assembly.Load("Lzq.Dashboard.Application"),
            ];
        MasaApp.TryAddAssemblies(assemblies);
    }
}
