using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Masa.BuildingBlocks.Data;
using Lzq.Core.Models;
using Lzq.WorkOrder.Application.Services.WorkOrder;
using Lzq.WorkOrder.Application.Contracts.IServices;
using Lzq.WorkOrder.Application.Contracts.ReferenceData;
using Lzq.WorkOrder.Domain.Entities.WorkOrder;
using Lzq.WorkOrder.Domain.IRepositories.WorkOrder;
using Lzq.WorkOrder.Domain.Repositories.WorkOrder;
using SqlSugar;

namespace Lzq.WorkOrder.Tests.Helpers;

/// <summary>
/// 服务测试基类 —— 为每个测试类提供独立的内存数据库 + 服务实例
/// 参考 BaseData 模块的 ServiceTestBase 实现
/// </summary>
public abstract class ServiceTestBase : IDisposable
{
    protected readonly TestDbContext Db;
    protected readonly SqlSugarScope Client;

    // Repositories
    protected readonly WorkOrderRepository WorkOrderRepo;
    protected readonly WorkReportRepository WorkReportRepo;

    // Mocked cross-module dependencies
    protected readonly Mock<IReferenceDataService> ReferenceDataServiceMock;

    // Services
    protected readonly WorkOrderService WorkOrderService;

    private readonly ServiceProvider _serviceProvider;

    protected ServiceTestBase()
    {
        // 1. 初始化内存数据库
        Db = new TestDbContext();
        Client = Db.Client;

        // 2. 创建仓储实例
        WorkOrderRepo = new WorkOrderRepository(Client);
        WorkReportRepo = new WorkReportRepository(Client);

        // 3. 创建跨模块服务 Mock
        ReferenceDataServiceMock = new Mock<IReferenceDataService>();
        SetupDefaultReferenceDataMocks();

        // 4. 构建测试用 ServiceProvider，并注册 IHttpContextAccessor 模拟
        var services = new ServiceCollection();

        // 注册所有仓储
        services.AddSingleton<IWorkOrderRepository>(WorkOrderRepo);
        services.AddSingleton<IWorkReportRepository>(WorkReportRepo);
        services.AddSingleton(ReferenceDataServiceMock.Object);

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
        WorkOrderService = new WorkOrderService();
    }

    /// <summary>
    /// 设置 ReferenceDataService 默认 Mock 行为：所有校验通过
    /// </summary>
    private void SetupDefaultReferenceDataMocks()
    {
        ReferenceDataServiceMock
            .Setup(s => s.LineExistsAsync(It.IsAny<long>()))
            .ReturnsAsync(true);

        ReferenceDataServiceMock
            .Setup(s => s.ProcessExistsAsync(It.IsAny<long>()))
            .ReturnsAsync(true);

        ReferenceDataServiceMock
            .Setup(s => s.GetLineByIdAsync(It.IsAny<long>()))
            .ReturnsAsync((long id) => new LineSimpleDto { Id = id, Name = $"产线{id}" });

        ReferenceDataServiceMock
            .Setup(s => s.GetLinesByIdsAsync(It.IsAny<List<long>>()))
            .ReturnsAsync((List<long> ids) => ids.Select(id => new LineSimpleDto { Id = id, Name = $"产线{id}" }).ToList());

        ReferenceDataServiceMock
            .Setup(s => s.GetProcessByIdAsync(It.IsAny<long>()))
            .ReturnsAsync((long id) => new ProcessSimpleDto { Id = id, Name = $"工序{id}" });

        ReferenceDataServiceMock
            .Setup(s => s.GetProcessesByIdsAsync(It.IsAny<List<long>>()))
            .ReturnsAsync((List<long> ids) => ids.Select(id => new ProcessSimpleDto { Id = id, Name = $"工序{id}" }).ToList());
    }

    protected async Task<List<WorkOrderEntity>> AllWorkOrdersAsync()
        => await Client.Queryable<WorkOrderEntity>().ToListAsync();

    public void Dispose()
    {
        Db.Dispose();
        _serviceProvider.Dispose();
    }
}
