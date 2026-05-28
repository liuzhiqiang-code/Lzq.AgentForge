using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.Entities.WorkOrder;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;
using Yitter.IdGenerator;

namespace Lzq.MES.Tests.Helpers;

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
            ConnectionString = $"DataSource=WoTest_{Guid.NewGuid():N};Mode=Memory;Cache=Shared",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = false,
            InitKeyType = InitKeyType.Attribute,
        });

        _client.CodeFirst.InitTables(
            typeof(WorkOrderEntity),
            typeof(WorkReportEntity),
            typeof(DashboardConfigEntity),
            typeof(QCOrderEntity),
            typeof(QCOrderItemEntity),
            typeof(LineEntity),
            typeof(WorkshopEntity),
            typeof(FactoryEntity),
            typeof(ProcessEntity),
            typeof(EquipmentEntity),
            typeof(RepairOrderEntity),
            typeof(DefectRecordEntity));
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
