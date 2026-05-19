using Lzq.Extensions.SqlSugar.Repository;
using Lzq.Temp.Domain.Entities.TestConfig;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.Temp.Domain.IRepositories.TestConfig;

public interface ITestConfigRepository : ISqlSugarRepository<TestConfigEntity>, ITransientDependency
{
    Task<TestConfigEntity?> GetByCodeAsync(string code);

    Task<List<TestConfigEntity>> GetByTypeAsync(int configType);
}
