using Lzq.Extensions.SqlSugar.Repository;
using Lzq.Temp.Domain.Entities.TestConfig;
using Lzq.Temp.Domain.IRepositories.TestConfig;

namespace Lzq.Temp.Domain.Repositories.TestConfig;

public class TestConfigRepository : SqlSugarRepository<TestConfigEntity>, ITestConfigRepository
{
    public async Task<TestConfigEntity?> GetByCodeAsync(string code)
    {
        return await AsQueryable().Where(x => x.Code == code).FirstAsync();
    }

    public async Task<List<TestConfigEntity>> GetByTypeAsync(int configType)
    {
        return await AsQueryable().Where(x => x.ConfigType == configType).ToListAsync();
    }
}
