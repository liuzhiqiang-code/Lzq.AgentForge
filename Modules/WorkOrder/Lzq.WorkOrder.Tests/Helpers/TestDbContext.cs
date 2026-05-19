using Lzq.WorkOrder.Domain.Entities.WorkOrder;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;
using Yitter.IdGenerator;

namespace Lzq.WorkOrder.Tests.Helpers;

/// <summary>
/// 测试数据库上下文 —— 基于 SQLite 内存数据库
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
            // 关键修复：IsAutoCloseConnection 必须为 false
            // - Cache=Shared 让多个连接共享同一个 in-memory 数据库
            // - IsAutoCloseConnection=false 防止连接关闭时数据库被销毁
            // - 使用唯一 GUID 确保测试间隔离
            ConnectionString = $"DataSource=WoTest_{Guid.NewGuid():N};Mode=Memory;Cache=Shared",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = false,  // 必须为 false
            InitKeyType = InitKeyType.Attribute,
        });

        _client.CodeFirst.InitTables(
            typeof(WorkOrderEntity),
            typeof(WorkReportEntity));
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
