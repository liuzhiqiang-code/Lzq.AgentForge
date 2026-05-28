using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Masa.BuildingBlocks.Data;
using Lzq.Core.Models;
using Lzq.MES.Application.Services;
using Lzq.MES.Application.Services.ReferenceData;
using Lzq.MES.Application.Services.WorkOrder;
using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Application.Contracts.IServices.WorkOrder;
using Lzq.MES.Application.Contracts.ReferenceData;
using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.Entities.WorkOrder;
using Lzq.MES.Domain.IRepositories;
using Lzq.MES.Domain.IRepositories.WorkOrder;
using Lzq.MES.Domain.Repositories;
using Lzq.MES.Domain.Repositories.WorkOrder;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Tests.Helpers;

public abstract class ServiceTestBase : IDisposable
{
    protected readonly TestDbContext Db;
    protected readonly SqlSugarScope Client;

    protected readonly WorkOrderRepository WorkOrderRepo;
    protected readonly WorkReportRepository WorkReportRepo;
    protected readonly DashboardConfigRepository DashboardConfigRepo;
    protected readonly QCOrderRepository QCOrderRepo;
    protected readonly QCOrderItemRepository QCOrderItemRepo;
    protected readonly LineRepository LineRepo;
    protected readonly WorkshopRepository WorkshopRepo;
    protected readonly FactoryRepository FactoryRepo;
    protected readonly ProcessRepository ProcessRepo;
    protected readonly EquipmentRepository EquipmentRepo;
    protected readonly RepairOrderRepository RepairOrderRepo;
    protected readonly DefectRecordRepository DefectRecordRepo;

    protected readonly ReferenceDataService ReferenceDataService;

    protected readonly Mock<IWorkOrderStatisticsService> WorkOrderStatisticsServiceMock;
    protected readonly Mock<IQCStatisticsService> QCStatisticsServiceMock;
    protected readonly Mock<IEquipmentStatisticsService> EquipmentStatisticsServiceMock;

    protected readonly WorkOrderService WorkOrderService;
    protected readonly KanbanService KanbanService;
    protected readonly QCOrderService QCOrderService;
    protected readonly LineService LineService;
    protected readonly WorkshopService WorkshopService;
    protected readonly FactoryService FactoryService;
    protected readonly ProcessService ProcessService;
    protected readonly EquipmentService EquipmentService;
    protected readonly RepairOrderService RepairOrderService;
    protected readonly DefectRecordService DefectRecordService;

    private readonly ServiceProvider _serviceProvider;

    protected ServiceTestBase()
    {
        Db = new TestDbContext();
        Client = Db.Client;

        WorkOrderRepo = new WorkOrderRepository(Client);
        WorkReportRepo = new WorkReportRepository(Client);
        DashboardConfigRepo = new DashboardConfigRepository(Client);
        QCOrderRepo = new QCOrderRepository(Client);
        QCOrderItemRepo = new QCOrderItemRepository(Client);
        LineRepo = new LineRepository(Client);
        WorkshopRepo = new WorkshopRepository(Client);
        FactoryRepo = new FactoryRepository(Client);
        ProcessRepo = new ProcessRepository(Client);
        EquipmentRepo = new EquipmentRepository(Client);
        RepairOrderRepo = new RepairOrderRepository(Client);
        DefectRecordRepo = new DefectRecordRepository(Client);

        ReferenceDataService = new ReferenceDataService(LineRepo, ProcessRepo);

        WorkOrderStatisticsServiceMock = new Mock<IWorkOrderStatisticsService>();
        QCStatisticsServiceMock = new Mock<IQCStatisticsService>();
        EquipmentStatisticsServiceMock = new Mock<IEquipmentStatisticsService>();

        var services = new ServiceCollection();
        services.AddMapster();

        services.AddSingleton<IWorkOrderRepository>(WorkOrderRepo);
        services.AddSingleton<IWorkReportRepository>(WorkReportRepo);
        services.AddSingleton<IDashboardConfigRepository>(DashboardConfigRepo);
        services.AddSingleton<IQCOrderRepository>(QCOrderRepo);
        services.AddSingleton<IQCOrderItemRepository>(QCOrderItemRepo);
        services.AddSingleton<ILineRepository>(LineRepo);
        services.AddSingleton<IWorkshopRepository>(WorkshopRepo);
        services.AddSingleton<IFactoryRepository>(FactoryRepo);
        services.AddSingleton<IProcessRepository>(ProcessRepo);
        services.AddSingleton<IEquipmentRepository>(EquipmentRepo);
        services.AddSingleton<IRepairOrderRepository>(RepairOrderRepo);
        services.AddSingleton<IDefectRecordRepository>(DefectRecordRepo);

        services.AddSingleton<ISqlSugarRepository<DashboardConfigEntity>>(DashboardConfigRepo);
        services.AddSingleton<ISqlSugarRepository<QCOrderEntity>>(QCOrderRepo);
        services.AddSingleton<ISqlSugarRepository<QCOrderItemEntity>>(QCOrderItemRepo);
        services.AddSingleton<ISqlSugarRepository<LineEntity>>(LineRepo);
        services.AddSingleton<ISqlSugarRepository<WorkshopEntity>>(WorkshopRepo);
        services.AddSingleton<ISqlSugarRepository<FactoryEntity>>(FactoryRepo);
        services.AddSingleton<ISqlSugarRepository<ProcessEntity>>(ProcessRepo);
        services.AddSingleton<ISqlSugarRepository<EquipmentEntity>>(EquipmentRepo);
        services.AddSingleton<ISqlSugarRepository<RepairOrderEntity>>(RepairOrderRepo);
        services.AddSingleton<ISqlSugarRepository<DefectRecordEntity>>(DefectRecordRepo);

        services.AddSingleton<IReferenceDataService>(ReferenceDataService);
        services.AddSingleton(WorkOrderStatisticsServiceMock.Object);
        services.AddSingleton(QCStatisticsServiceMock.Object);
        services.AddSingleton(EquipmentStatisticsServiceMock.Object);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequestServices = new Mock<IServiceProvider>();

        _serviceProvider = services.BuildServiceProvider();
        mockRequestServices
            .Setup(sp => sp.GetService(It.IsAny<Type>()))
            .Returns<Type>(type => _serviceProvider.GetService(type));
        mockHttpContext.Setup(ctx => ctx.RequestServices).Returns(mockRequestServices.Object);
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        services.AddSingleton(mockHttpContextAccessor.Object);
        _serviceProvider = services.BuildServiceProvider();

        MasaApp.SetServiceCollection(services);
        MasaApp.Build(_serviceProvider);

        WorkOrderService = new WorkOrderService();
        KanbanService = new KanbanService();
        QCOrderService = new QCOrderService();
        LineService = new LineService();
        WorkshopService = new WorkshopService();
        FactoryService = new FactoryService();
        ProcessService = new ProcessService();
        EquipmentService = new EquipmentService();
        RepairOrderService = new RepairOrderService();
        DefectRecordService = new DefectRecordService();
    }

    protected async Task<List<WorkOrderEntity>> AllWorkOrdersAsync()
        => await Client.Queryable<WorkOrderEntity>().ToListAsync();

    protected async Task<List<DashboardConfigEntity>> AllConfigsAsync()
        => await Client.Queryable<DashboardConfigEntity>().ToListAsync();

    protected async Task<List<QCOrderEntity>> AllQCOrdersAsync()
        => await Client.Queryable<QCOrderEntity>().ToListAsync();

    protected async Task<List<LineEntity>> AllLinesAsync()
        => await Client.Queryable<LineEntity>().ToListAsync();

    protected async Task<List<WorkshopEntity>> AllWorkshopsAsync()
        => await Client.Queryable<WorkshopEntity>().ToListAsync();

    protected async Task<List<FactoryEntity>> AllFactoriesAsync()
        => await Client.Queryable<FactoryEntity>().ToListAsync();

    protected async Task<List<ProcessEntity>> AllProcessesAsync()
        => await Client.Queryable<ProcessEntity>().ToListAsync();

    protected async Task<List<EquipmentEntity>> AllEquipmentsAsync()
        => await Client.Queryable<EquipmentEntity>().ToListAsync();

    protected async Task<List<RepairOrderEntity>> AllRepairOrdersAsync()
        => await Client.Queryable<RepairOrderEntity>().ToListAsync();

    protected async Task<List<DefectRecordEntity>> AllDefectRecordsAsync()
        => await Client.Queryable<DefectRecordEntity>().ToListAsync();

    public void Dispose()
    {
        Db.Dispose();
        _serviceProvider.Dispose();
    }
}
