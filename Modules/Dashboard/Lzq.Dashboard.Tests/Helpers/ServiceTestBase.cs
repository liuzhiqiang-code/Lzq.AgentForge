using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Masa.BuildingBlocks.Data;
using Lzq.WorkOrder.Application.Contracts.IServices.WorkOrder;
using Lzq.Extensions.SqlSugar.Repository;
using Lzq.Extensions.Redis;
using Microsoft.Extensions.Logging;
using SqlSugar;
using Lzq.Dashboard.Domain.Entities;
using Lzq.Dashboard.Domain.IRepositories;
using Lzq.Dashboard.Domain.Repositories;
using Lzq.Equipment.Application.Contracts.IServices;
using Lzq.QA.Application.Contracts.IServices;
using Lzq.Dashboard.Application.Services;

namespace Lzq.Dashboard.Tests.Helpers;

/// <summary>
/// 看板服务测试基类 —— 为每个测试类提供独立的内存数据库 + 服务实例
/// 参考 BaseData 模块的 ServiceTestBase 实现
/// </summary>
public abstract class ServiceTestBase : IDisposable
{
    protected readonly TestDbContext Db;
    protected readonly SqlSugarScope Client;

    // Repository
    protected readonly DashboardConfigRepository ConfigRepo;

    // Service
    protected readonly KanbanService KanbanService;

    // Moq mocks for cross-module dependencies
    protected readonly Mock<IWorkOrderStatisticsService> WorkOrderStatsMock;
    protected readonly Mock<IQCStatisticsService> QCStatsMock;
    protected readonly Mock<IEquipmentStatisticsService> EquipmentStatsMock;
    protected readonly Mock<ILogger<KanbanService>> LoggerMock;

    private readonly ServiceProvider _serviceProvider;

    protected ServiceTestBase()
    {
        // 0. 重置 MasaApp 静态引用，防止上个测试残留影响
        MasaApp.SetServiceCollection(new ServiceCollection());

        // 1. 初始化内存数据库
        Db = new TestDbContext();
        Client = Db.Client;

        // 2. 创建仓储实例
        ConfigRepo = new DashboardConfigRepository(Client);

        // 3. 创建跨模块服务 Mock
        WorkOrderStatsMock = new Mock<IWorkOrderStatisticsService>();
        QCStatsMock = new Mock<IQCStatisticsService>();
        EquipmentStatsMock = new Mock<IEquipmentStatisticsService>();
        LoggerMock = new Mock<ILogger<KanbanService>>();

        // 4. 构建测试用 ServiceProvider，并注册 IHttpContextAccessor 模拟
        var services = new ServiceCollection();
        services.AddMapster();

        // 注册所有仓储
        services.AddSingleton<IDashboardConfigRepository>(ConfigRepo);

        // 注册通用仓储（KanbanService 直接注入 ISqlSugarRepository<DashboardConfigEntity>）
        services.AddSingleton<ISqlSugarRepository<DashboardConfigEntity>>(
            new SqlSugarRepository<DashboardConfigEntity>(Client));

        // 注册 Mock 服务
        services.AddSingleton(WorkOrderStatsMock.Object);
        services.AddSingleton(QCStatsMock.Object);
        services.AddSingleton(EquipmentStatsMock.Object);
        services.AddSingleton(LoggerMock.Object);

        // 注入 TestRedisClient（ServiceBase 通过 GetRequiredService 解析）
        services.AddSingleton<ILzqRedisClient>(new TestRedisClient());

        // 模拟 IHttpContextAccessor，使其返回的 RequestServices 指向本 ServiceProvider
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequestServices = new Mock<IServiceProvider>();

        // 让 RequestServices 实际从我们的 ServiceProvider 解析
        _serviceProvider = services.BuildServiceProvider();
        mockRequestServices
            .Setup(sp => sp.GetService(It.IsAny<Type>()))
            .Returns<Type>(type => _serviceProvider.GetService(type));
        mockHttpContext.Setup(ctx => ctx.RequestServices).Returns(mockRequestServices.Object);
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        // 把 IHttpContextAccessor 也注册进去
        services.AddSingleton(mockHttpContextAccessor.Object);

        // 重新构建包含 Accessor 的 ServiceProvider
        _serviceProvider = services.BuildServiceProvider();

        // 设置为全局根容器，ServiceBase 会通过 MasaApp 获取 IHttpContextAccessor
        MasaApp.SetServiceCollection(services);
        MasaApp.Build(_serviceProvider);

        // 5. 创建服务实例（依赖会自动从 RequestServices 解析）
        KanbanService = new KanbanService();
    }

    protected async Task<List<DashboardConfigEntity>> AllConfigsAsync()
        => await Client.Queryable<DashboardConfigEntity>().ToListAsync();

    public void Dispose()
    {
        Db.Dispose();
        _serviceProvider.Dispose();
    }

    /// <summary>
    /// 测试用 Redis 客户端桩 —— 确保 GetOrSetAsync 执行工厂委托而非返回 null
    /// </summary>
    private sealed class TestRedisClient : ILzqRedisClient
    {
        public T? Get<T>(string key) => default;
        public Task<T?> GetAsync<T>(string key) => Task.FromResult<T?>(default);
        public void Set<T>(string key, T value, TimeSpan? expiry = null) { }
        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) => Task.CompletedTask;
        public bool Remove(string key) => true;
        public Task<bool> RemoveAsync(string key) => Task.FromResult(true);
        public bool Exists(string key) => false;
        public Task<bool> ExistsAsync(string key) => Task.FromResult(false);
        public IDisposable Lock(string resourceKey, int timeoutSeconds = 10) => new MemoryStream();
        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> dataRetriever, TimeSpan? expiry = null)
            => await dataRetriever();
    }
}
