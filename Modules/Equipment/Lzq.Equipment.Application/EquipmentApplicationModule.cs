using Lzq.Core.Modules;
using Lzq.Extensions.SqlSugar;
using Masa.BuildingBlocks.Data;
using System.Reflection;

namespace Lzq.Equipment.Application;

[DependsOn(typeof(SqlSugarModule))]
public class EquipmentApplicationModule : BaseModule
{
    public override void Configure(ModuleConfigureContext context)
    {
        Assembly[] assemblies = [
            Assembly.Load("Lzq.Equipment.Domain"),
            Assembly.Load("Lzq.Equipment.Application.Contracts"),
            Assembly.Load("Lzq.Equipment.Application"),
            ];
        MasaApp.TryAddAssemblies(assemblies);
    }
}
