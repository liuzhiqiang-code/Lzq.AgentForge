using Lzq.Core.Modules;
using Lzq.Extensions.SqlSugar;
using Masa.BuildingBlocks.Data;
using System.Reflection;

namespace Lzq.QA.Application;

[DependsOn(typeof(SqlSugarModule))]
public class QAApplicationModule : BaseModule
{
    public override void Configure(ModuleConfigureContext context)
    {
        Assembly[] assemblies = [
            Assembly.Load("Lzq.QA.Domain"),
            Assembly.Load("Lzq.QA.Application.Contracts"),
            Assembly.Load("Lzq.QA.Application"),
            ];
        MasaApp.TryAddAssemblies(assemblies);
    }
}
