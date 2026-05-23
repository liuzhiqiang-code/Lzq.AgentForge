using Lzq.Core.Modules;
using Lzq.Extensions.AI;
using Lzq.Extensions.SqlSugar;
using Masa.BuildingBlocks.Data;
using System.Reflection;

namespace Lzq.AI.Application;

[DependsOn(typeof(SqlSugarModule), typeof(AIModule))]
public class AIApplicationModule : BaseModule
{
    public override void Configure(ModuleConfigureContext context)
    {
        Assembly[] assemblies = [
            Assembly.Load("Lzq.AI.Domain"),
            Assembly.Load("Lzq.AI.Application.Contracts"),
            Assembly.Load("Lzq.AI.Application"),
            ];
        MasaApp.TryAddAssemblies(assemblies);
    }
}
