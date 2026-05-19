using Lzq.BaseData.Domain.Entities;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;
using Yitter.IdGenerator;

namespace Lzq.BaseData.Tests.Helpers;

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
            // 修复说明：
            // - DataSource={guid} 确保每个测试实例有独立的内存数据库（测试隔离）
            // - Mode=Memory 使用内存数据库
            // - Cache=Shared 允许多个连接共享同一个 in-memory 数据库
            // - IsAutoCloseConnection=false 关键修复：防止连接关闭时数据库被销毁
            //   当 IsAutoCloseConnection=true 时，InitTables 后的连接关闭会导致
            //   in-memory 数据库被销毁，后续查询无法找到表
            ConnectionString = $"DataSource=TestDb_{Guid.NewGuid():N};Mode=Memory;Cache=Shared",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = false,  // 必须为 false
            InitKeyType = InitKeyType.Attribute,
        });

        // 创建表
        _client.CodeFirst.InitTables(
            typeof(FactoryEntity),
            typeof(WorkshopEntity),
            typeof(LineEntity),
            typeof(ProcessEntity));
    }

    /// <summary>
    /// 创建真实 SqlSugarRepository 实例（连接测试内存数据库）
    /// </summary>
    public SqlSugarRepository<T> CreateRepository<T>() where T : class, new()
    {
        return new SqlSugarRepository<T>(_client);
    }

    /// <summary>
    /// 向内存数据库批量插入种子数据
    /// </summary>
    public void Seed<T>(params T[] entities) where T : class, new()
    {
        if (entities.Length > 0)
            _client.Insertable(entities).ExecuteCommand();
    }

    /// <summary>
    /// 向内存数据库批量插入多种类型的种子数据
    /// 注意：SqlSugar 的 Insertable<T> 不支持 object 类型，需使用 InsertableByObject
    /// </summary>
    public void Seed(params object[] entities)
    {
        if (entities.Length > 0)
        {
            foreach (var entity in entities)
            {
                _client.InsertableByObject(entity).ExecuteCommand();
            }
        }
    }

    public void Dispose()
    {
        _client.Close();
        _client.Dispose();
    }
}
