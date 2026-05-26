using Lzq.Core.Modules;
using Lzq.WorkOrder.Application.Contracts;

namespace Lzq.WorkOrder.Application;

[DependsOn(typeof(WorkOrderApplicationContractsModule))]
public class WorkOrderApplicationModule : BaseModule
{
}
