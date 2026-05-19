using Lzq.Extensions.SqlSugar.Repository;
using Lzq.Temp.Domain.Entities.TestConfig;
using SqlSugar;
using Yitter.IdGenerator;

namespace Lzq.Temp.Tests.Helpers;

public sealed class TestDbContext : IDisposable
{
    private readonly SqlSugarScope _client;

    public SqlSugarScope Client => _client;

    static TestDbContext()
    {
        YitIdHelper.SetIdGenerator(new IdGeneratorOptions(1)
        {
            WorkerId = 1,
            WorkerIdBitLength = 6,
            SeqBitLength = 6,
        });
    }

    public TestDbContext()
    {
        _client = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"DataSource=TempTest_{Guid.NewGuid():N};Mode=Memory;Cache=Shared",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = false,
            InitKeyType = InitKeyType.Attribute,
        });

        _client.CodeFirst.InitTables(typeof(TestConfigEntity));
    }

    public void Seed<T>(params T[] entities) where T : class, new()
    {
        if (entities.Length > 0)
            _client.Insertable(entities).ExecuteCommand();
    }

    public void Dispose()
    {
        _client.Close();
        _client.Dispose();
    }
}
