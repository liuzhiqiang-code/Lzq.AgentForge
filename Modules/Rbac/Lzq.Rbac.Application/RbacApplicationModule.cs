using Lzq.Core.Modules;
using Lzq.Extensions.SqlSugar;
using Masa.BuildingBlocks.Data;
using System.Reflection;

namespace Lzq.Rbac.Application;

[DependsOn(typeof(SqlSugarModule))]
public class RbacApplicationModule : BaseModule
{
    public override void Configure(ModuleConfigureContext context)
    {
        Assembly[] assemblies = [
            Assembly.Load("Lzq.Rbac.Domain"),
            Assembly.Load("Lzq.Rbac.Application.Contracts"),
            Assembly.Load("Lzq.Rbac.Application"),
            ];
        MasaApp.TryAddAssemblies(assemblies);
    }
}
