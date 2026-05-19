using Lzq.Dashboard.Domain.Entities;
using Lzq.Dashboard.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.Dashboard.Domain.Repositories;

/// <summary>
/// 看板配置仓储实现
/// </summary>
public class DashboardConfigRepository : SqlSugarRepository<DashboardConfigEntity>, IDashboardConfigRepository
{
    public DashboardConfigRepository()
    {
    }
    public DashboardConfigRepository(ISqlSugarClient client) : base(client) { }

    public async Task<DashboardConfigEntity?> GetByCodeAsync(string code)
    {
        return await AsQueryable()
            .Where(x => x.Code == code)
            .FirstAsync();
    }

    public async Task<List<DashboardConfigEntity>> GetEnabledConfigsAsync()
    {
        return await AsQueryable()
            .Where(x => x.IsEnabled == true)
            .ToListAsync();
    }

    public async Task<bool> ExistsByCodeAsync(string code, long? excludeId = null)
    {
        var query = AsQueryable()
            .Where(x => x.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
