using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Masa.BuildingBlocks.Data;
using Lzq.BaseData.Application.Services;
using Lzq.BaseData.Domain.Entities;
using Lzq.BaseData.Domain.IRepositories;
using Lzq.BaseData.Domain.Repositories;
using Lzq.Extensions.Redis;
using SqlSugar;

namespace Lzq.BaseData.Tests.Helpers;

/// <summary>
/// 服务测试基类 —— 为每个测试类提供独立的内存数据库 + 服务实例
/// </summary>
public abstract class ServiceTestBase : IDisposable
{
    protected readonly TestDbContext Db;
    protected readonly SqlSugarScope Client;

    // Repository instances
    protected readonly FactoryRepository FactoryRepo;
    protected readonly WorkshopRepository WorkshopRepo;
    protected readonly LineRepository LineRepo;
    protected readonly ProcessRepository ProcessRepo;

    // Service instances
    protected readonly FactoryService FactoryService;
    protected readonly WorkshopService WorkshopService;
    protected readonly LineService LineService;
    protected readonly ProcessService ProcessService;

    private readonly ServiceProvider _serviceProvider;

    protected ServiceTestBase()
    {
        // 0. 重置 MasaApp 静态引用，防止上个测试残留影响
        MasaApp.SetServiceCollection(new ServiceCollection());

        // 1. 初始化内存数据库
        Db = new TestDbContext();
        Client = Db.Client;

        // 2. 创建仓储实例
        FactoryRepo = new FactoryRepository(Client);
        WorkshopRepo = new WorkshopRepository(Client);
        LineRepo = new LineRepository(Client);
        ProcessRepo = new ProcessRepository(Client);

        // 3. 构建测试用 ServiceProvider，并注册 IHttpContextAccessor 模拟
        var services = new ServiceCollection();
        services.AddMapster();

        // 注册所有仓储
        services.AddSingleton<IFactoryRepository>(FactoryRepo);
        services.AddSingleton<IWorkshopRepository>(WorkshopRepo);
        services.AddSingleton<ILineRepository>(LineRepo);
        services.AddSingleton<IProcessRepository>(ProcessRepo);

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

        // 4. 创建服务实例（依赖会自动从 RequestServices 解析）
        FactoryService = new FactoryService();
        WorkshopService = new WorkshopService();
        LineService = new LineService();
        ProcessService = new ProcessService();
    }

    protected async Task<List<FactoryEntity>> AllFactoriesAsync()
        => await Client.Queryable<FactoryEntity>().ToListAsync();

    protected async Task<List<WorkshopEntity>> AllWorkshopsAsync()
        => await Client.Queryable<WorkshopEntity>().ToListAsync();

    protected async Task<List<LineEntity>> AllLinesAsync()
        => await Client.Queryable<LineEntity>().ToListAsync();

    protected async Task<List<ProcessEntity>> AllProcessesAsync()
        => await Client.Queryable<ProcessEntity>().ToListAsync();

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
