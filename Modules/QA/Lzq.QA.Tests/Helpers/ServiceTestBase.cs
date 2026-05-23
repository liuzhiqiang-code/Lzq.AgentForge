using Lzq.QA.Application.Services;
using Lzq.QA.Domain.Entities;
using Lzq.QA.Domain.IRepositories;
using Lzq.QA.Domain.Repositories;
using Masa.BuildingBlocks.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SqlSugar;
using System.Reflection;
using Yitter.IdGenerator;

namespace Lzq.QA.Tests.Helpers;

/// <summary>
/// 服务测试基类 — 为每个测试类提供独立的内存数据库 + 服务实例
/// 参考 BaseData 模块的 ServiceTestBase 实现
/// </summary>
public abstract class ServiceTestBase : IDisposable
{
    protected readonly TestDbContext Db;
    protected readonly SqlSugarScope Client;

    // Repositories
    protected readonly DefectRecordRepository DefectRepo;
    protected readonly QCOrderRepository QCOrderRepo;
    protected readonly QCOrderItemRepository QCOrderItemRepo;

    // Services
    protected readonly DefectRecordService DefectService;
    protected readonly QCOrderService QCOrderService;

    private readonly ServiceProvider _serviceProvider;

    protected ServiceTestBase()
    {
        // 0. 重置 MasaApp 静态引用，防止上个测试残留影响
        MasaApp.SetServiceCollection(new ServiceCollection());

        // 1. 初始化内存数据库
        Db = new TestDbContext();
        Client = Db.Client;

        // 2. 创建仓储实例
        DefectRepo = new DefectRecordRepository(Client);
        QCOrderRepo = new QCOrderRepository(Client);
        QCOrderItemRepo = new QCOrderItemRepository(Client);

        // 3. 构建测试用 ServiceProvider，并注册 IHttpContextAccessor 模拟
        var services = new ServiceCollection();

        // 注册所有仓储
        services.AddSingleton<IDefectRecordRepository>(DefectRepo);
        services.AddSingleton<IQCOrderRepository>(QCOrderRepo);
        services.AddSingleton<IQCOrderItemRepository>(QCOrderItemRepo);

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
        DefectService = new DefectRecordService();
        QCOrderService = new QCOrderService();
    }

    protected async Task<List<DefectRecordEntity>> AllDefectsAsync()
        => await Client.Queryable<DefectRecordEntity>().ToListAsync();

    protected async Task<List<QCOrderEntity>> AllQCOrdersAsync()
        => await Client.Queryable<QCOrderEntity>().ToListAsync();

    public void Dispose()
    {
        Db.Dispose();
    }
}
