using Lzq.Core.Modules;
using Lzq.Extensions.SqlSugar;

namespace Lzq.WorkOrder.Domain;

[DependsOn(typeof(SqlSugarModule))]
public class WorkOrderDomainModule : BaseModule
{
}
