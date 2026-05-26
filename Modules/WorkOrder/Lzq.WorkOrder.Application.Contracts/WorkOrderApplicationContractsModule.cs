using Lzq.Core.Modules;
using Lzq.WorkOrder.Domain;

namespace Lzq.WorkOrder.Application.Contracts;

[DependsOn(typeof(WorkOrderDomainModule))]
public class WorkOrderApplicationContractsModule : BaseModule
{
}
