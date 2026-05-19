using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Masa.BuildingBlocks.Data;
using Lzq.Temp.Application.Services.TestConfig;
using Lzq.Temp.Domain.Entities.TestConfig;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.Temp.Tests.Helpers;

public abstract class ServiceTestBase : IDisposable
{
    protected readonly TestDbContext Db;
    protected readonly SqlSugarScope Client;

    protected readonly TestConfigService TestConfigService;

    private readonly ServiceProvider _serviceProvider;

    protected ServiceTestBase()
    {
        MasaApp.SetServiceCollection(new ServiceCollection());

        Db = new TestDbContext();
        Client = Db.Client;

        var services = new ServiceCollection();
        services.AddMapster();

        services.AddSingleton<ISqlSugarRepository<TestConfigEntity>>(
            new SqlSugarRepository<TestConfigEntity>(Client));

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

        TestConfigService = new TestConfigService();
    }

    protected async Task<List<TestConfigEntity>> AllConfigsAsync()
        => await Client.Queryable<TestConfigEntity>().ToListAsync();

    public void Dispose()
    {
        Db.Dispose();
        _serviceProvider.Dispose();
    }
}
