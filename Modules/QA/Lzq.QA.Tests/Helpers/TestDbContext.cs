using Lzq.Extensions.SqlSugar.Repository;
using Lzq.QA.Domain.Entities;
using SqlSugar;
using Yitter.IdGenerator;

namespace Lzq.QA.Tests.Helpers;

/// <summary>
/// 测试数据库上下文 — 基于 SQLite 内存数据库
/// 无 ConfigId，实体 [Tenant("AgentForge")] 自动回退到默认连接
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
            ConnectionString = $"DataSource=EquipTest_{Guid.NewGuid():N};Mode=Memory;Cache=Shared",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = false,
            InitKeyType = InitKeyType.Attribute,
        });

        _client.CodeFirst.InitTables(
            typeof(DefectRecordEntity),
            typeof(QCOrderEntity),
            typeof(QCOrderItemEntity));
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
