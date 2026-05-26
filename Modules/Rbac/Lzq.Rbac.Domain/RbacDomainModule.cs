using Lzq.Core.Modules;
using Lzq.Extensions.SqlSugar;

namespace Lzq.Rbac.Domain;

[DependsOn(typeof(SqlSugarModule))]
public class RbacDomainModule : BaseModule
{
}
