using Lzq.Dashboard.Domain.Entities;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;
using Yitter.IdGenerator;

namespace Lzq.Dashboard.Tests.Helpers;

/// <summary>
/// 测试数据库上下文 —— 基于 SQLite 内存数据库，每次测试拥有独立实例
/// </summary>
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
            ConnectionString = $"DataSource=DashboardTest_{Guid.NewGuid():N};Mode=Memory;Cache=Shared",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = false,
            InitKeyType = InitKeyType.Attribute,
        });

        // 创建表
        _client.CodeFirst.InitTables(
            typeof(DashboardConfigEntity));
    }

    public SqlSugarRepository<T> CreateRepository<T>() where T : class, new()
    {
        return new SqlSugarRepository<T>(_client);
    }

    public void Seed<T>(params T[] entities) where T : class, new()
    {
        if (entities.Length > 0)
            _client.Insertable(entities).ExecuteCommand();
    }

    public void Seed(params object[] entities)
    {
        if (entities.Length > 0)
        {
            foreach (var entity in entities)
                _client.Insertable(entity).ExecuteCommand();
        }
    }

    public void Dispose()
    {
        _client.Close();
        _client.Dispose();
    }
}
