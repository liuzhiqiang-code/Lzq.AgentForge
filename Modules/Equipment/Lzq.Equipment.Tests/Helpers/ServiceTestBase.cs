using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Masa.BuildingBlocks.Data;
using SqlSugar;
using Lzq.Equipment.Domain.Entities;
using Lzq.Equipment.Domain.IRepositories;
using Lzq.Equipment.Domain.Repositories;
using Lzq.Equipment.Application.Services;

namespace Lzq.Equipment.Tests.Helpers;

/// <summary>
/// 服务测试基类 —— 为每个测试类提供独立的内存数据库 + 服务实例
/// 参考 BaseData 模块的 ServiceTestBase 实现
/// </summary>
public abstract class ServiceTestBase : IDisposable
{
    protected readonly TestDbContext Db;
    protected readonly SqlSugarScope Client;

    // Repositories
    protected readonly EquipmentRepository EquipmentRepo;
    protected readonly InspectionPlanRepository InspectionPlanRepo;
    protected readonly InspectionRecordRepository InspectionRecordRepo;
    protected readonly InspectionItemRepository InspectionItemRepo;
    protected readonly RepairOrderRepository RepairOrderRepo;
    protected readonly MaintenancePlanRepository MaintenancePlanRepo;
    protected readonly MaintenanceRecordRepository MaintenanceRecordRepo;

    // Services
    protected readonly EquipmentService EquipmentService;
    protected readonly RepairOrderService RepairOrderService;

    private readonly ServiceProvider _serviceProvider;

    protected ServiceTestBase()
    {
        // 0. 重置 MasaApp 静态引用，防止上个测试残留影响
        MasaApp.SetServiceCollection(new ServiceCollection());

        // 1. 初始化内存数据库
        Db = new TestDbContext();
        Client = Db.Client;

        // 2. 创建仓储实例
        EquipmentRepo = new EquipmentRepository(Client);
        InspectionPlanRepo = new InspectionPlanRepository(Client);
        InspectionRecordRepo = new InspectionRecordRepository(Client);
        InspectionItemRepo = new InspectionItemRepository(Client);
        RepairOrderRepo = new RepairOrderRepository(Client);
        MaintenancePlanRepo = new MaintenancePlanRepository(Client);
        MaintenanceRecordRepo = new MaintenanceRecordRepository(Client);

        // 3. 构建测试用 ServiceProvider，并注册 IHttpContextAccessor 模拟
        var services = new ServiceCollection();
        services.AddMapster();

        // 注册所有仓储
        services.AddSingleton<IEquipmentRepository>(EquipmentRepo);
        services.AddSingleton<IInspectionPlanRepository>(InspectionPlanRepo);
        services.AddSingleton<IInspectionRecordRepository>(InspectionRecordRepo);
        services.AddSingleton<IInspectionItemRepository>(InspectionItemRepo);
        services.AddSingleton<IRepairOrderRepository>(RepairOrderRepo);
        services.AddSingleton<IMaintenancePlanRepository>(MaintenancePlanRepo);
        services.AddSingleton<IMaintenanceRecordRepository>(MaintenanceRecordRepo);

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
        EquipmentService = new EquipmentService();
        RepairOrderService = new RepairOrderService();
    }

    protected async Task<List<EquipmentEntity>> AllEquipmentsAsync()
        => await Client.Queryable<EquipmentEntity>().ToListAsync();

    protected async Task<List<RepairOrderEntity>> AllRepairOrdersAsync()
        => await Client.Queryable<RepairOrderEntity>().ToListAsync();

    public void Dispose()
    {
        Db.Dispose();
        _serviceProvider.Dispose();
    }
}
