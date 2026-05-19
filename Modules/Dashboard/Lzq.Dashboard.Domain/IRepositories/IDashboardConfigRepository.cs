using Lzq.Dashboard.Domain.Entities;
using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.Dashboard.Domain.IRepositories;

/// <summary>
/// 看板配置仓储接口
/// </summary>
public interface IDashboardConfigRepository : ISqlSugarRepository<DashboardConfigEntity>, ITransientDependency
{
    /// <summary>
    /// 根据编码查询
    /// </summary>
    Task<DashboardConfigEntity?> GetByCodeAsync(string code);

    /// <summary>
    /// 获取启用的配置列表
    /// </summary>
    Task<List<DashboardConfigEntity>> GetEnabledConfigsAsync();

    /// <summary>
    /// 检查编码是否存在
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, long? excludeId = null);
}
