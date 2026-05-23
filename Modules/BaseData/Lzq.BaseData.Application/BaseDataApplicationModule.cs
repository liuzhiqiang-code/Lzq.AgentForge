using Lzq.Core.Modules;
using Lzq.Extensions.SqlSugar;
using Masa.BuildingBlocks.Data;
using System.Reflection;

namespace Lzq.BaseData.Application;

[DependsOn(typeof(SqlSugarModule))]
public class BaseDataApplicationModule : BaseModule
{
    public override void Configure(ModuleConfigureContext context)
    {
        Assembly[] assemblies = [
            Assembly.Load("Lzq.BaseData.Domain"),
            Assembly.Load("Lzq.BaseData.Application.Contracts"),
            Assembly.Load("Lzq.BaseData.Application"),
            ];
        MasaApp.TryAddAssemblies(assemblies);
    }
}
