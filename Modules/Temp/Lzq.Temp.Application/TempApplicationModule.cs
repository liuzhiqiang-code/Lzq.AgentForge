using Lzq.Core.Modules;
using Lzq.Extensions.SqlSugar;
using Masa.BuildingBlocks.Data;
using System.Reflection;

namespace Lzq.Temp.Application;

[DependsOn(typeof(SqlSugarModule))]
public class TempApplicationModule : BaseModule
{
    public override void Configure(ModuleConfigureContext context)
    {
        Assembly[] assemblies = [
            Assembly.Load("Lzq.Temp.Domain"),
            Assembly.Load("Lzq.Temp.Application.Contracts"),
            Assembly.Load("Lzq.Temp.Application"),
            ];
        MasaApp.TryAddAssemblies(assemblies);
    }
}
