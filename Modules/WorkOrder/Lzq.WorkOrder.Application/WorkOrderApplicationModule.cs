using Lzq.Core.Modules;
using Lzq.Extensions.SqlSugar;
using Masa.BuildingBlocks.Data;
using System.Reflection;

namespace Lzq.WorkOrder.Application;

[DependsOn(typeof(SqlSugarModule))]
public class WorkOrderApplicationModule : BaseModule
{
    public override void Configure(ModuleConfigureContext context)
    {
        Assembly[] assemblies = [
            Assembly.Load("Lzq.WorkOrder.Domain"),
            Assembly.Load("Lzq.WorkOrder.Application.Contracts"),
            Assembly.Load("Lzq.WorkOrder.Application"),
            ];
        MasaApp.TryAddAssemblies(assemblies);
    }
}
