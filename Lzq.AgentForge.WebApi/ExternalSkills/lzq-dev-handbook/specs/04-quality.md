# 开发规范与质量保障

---

## 审查清单

AI Agent 完成代码生成后，必须检查：

- ✅ 实体继承 `BaseFullEntity`，非 `BaseEntity`
- ✅ 每个实体独立文件
- ✅ using 命名空间正确（见 §15 速查表）
- ✅ Service 实现接口，返回类型匹配
- ✅ 接口中无 `[FromBody]`/`[FromQuery]`；Service 实现中复杂类型加 `[FromBody]`，简单类型加 `[FromQuery]`（详见 02-core-library.md §参数绑定）
- ✅ 排序使用链式 `OrderByDescending`，非 `ThenBy(OrderByType)`
- ✅ 验证器使用 `FluentValidation`，非错误命名空间
- ✅ csproj 包含完整 PropertyGroup

---

## 单元测试 ServiceTestBase 规范 🚨

### 必须引用的命名空间

```csharp
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Masa.BuildingBlocks.Data;
using Lzq.{Module}.Application.Services.{Entity};
using Lzq.{Module}.Domain.Entities.{Entity};
using Lzq.{Module}.Domain.IRepositories.{Entity};
using Lzq.{Module}.Domain.Repositories.{Entity};
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;
```

### 标准 ServiceTestBase 模板

```csharp
namespace Lzq.{Module}.Tests.Helpers;

/// <summary>
/// 服务测试基类 —— 为每个测试类提供独立的内存数据库 + 服务实例
/// 参考 BaseData 模块的 ServiceTestBase 实现
/// </summary>
public abstract class ServiceTestBase : IDisposable
{
    protected readonly TestDbContext Db;
    protected readonly SqlSugarScope Client;

    // Repositories
    protected readonly {Entity}Repository {Entity}Repo;

    // Services
    protected readonly {Entity}Service {Entity}Service;

    private readonly ServiceProvider _serviceProvider;  // ✅ 必须有，用于 Dispose

    protected ServiceTestBase()
    {
        // 0. 清空旧的 MasaApp 引用，避免测试间相互影响 ⚠️ 关键步骤
        MasaApp.SetServiceCollection(new ServiceCollection());

        // 1. 初始化内存数据库
        Db = new TestDbContext();
        Client = Db.Client;

        // 2. 创建仓储实例
        {Entity}Repo = new {Entity}Repository(Client);

        // 3. 构建测试用 ServiceProvider，并注册 IHttpContextAccessor 模拟
        var services = new ServiceCollection();
        services.AddMapster();  // ✅ 必须添加 Mapster
        services.AddCoreAssembly("Lzq.");  // ✅ 自动扫描注册

        // 注册所有仓储
        services.AddSingleton<I{Entity}Repository>({Entity}Repo);

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

        // 4. 创建服务实例（依赖会自动从 RequestServices 解析，无需反射注入）
        {Entity}Service = new {Entity}Service();
    }

    protected async Task<List<{Entity}Entity>> All{Entities}Async()
        => await Client.Queryable<{Entity}Entity>().ToListAsync();

    public void Dispose()
    {
        Db.Dispose();
        _serviceProvider.Dispose();  // ✅ 必须清理 ServiceProvider
    }
}
```

### 关键要点

| 要点 | 说明 |
|------|------|
| **步骤 0** | `MasaApp.SetServiceCollection(new ServiceCollection())` 清空旧引用 |
| `_serviceProvider` 字段 | **必须有**，用于 Dispose 清理 |
| `services.AddMapster()` | 必须添加，否则映射失败 |
| `services.AddCoreAssembly("Lzq.")` | 测试环境自动扫描注册 |
| `IHttpContextAccessor` Mock | **核心配置**，使 Service 直接 new 后自动获取依赖 |
| `MasaApp.SetServiceCollection` | 必须设置，否则 `GetRequiredService` 无法工作 |
| `new Service()` | 直接 new，**无需反射注入** |
| `_serviceProvider.Dispose()` | 必须清理，否则内存泄漏 |

### 测试规范补充说明 🚨

从实际测试修复中总结的关键规范：

1. **多个测试类共享 MasaApp 静态状态时**：必须加 `[Collection("相同名称")]` 确保串行执行
   ```csharp
   [Collection("WorkOrder.Tests")]
   public class WorkReportServiceTests : ServiceTestBase
   {
       // 测试类内容
   }
   ```

2. **测试基类构造函数开头**应调用 `MasaApp.SetServiceCollection(new ServiceCollection())` 清空旧引用

3. **永远不要删除 bin/obj 后执行 restore**：使用 `dotnet test --no-restore`**

4. **永远不要执行 `dotnet nuget locals all --clear`**：会破坏 .NET SDK 的 NuGet 基础设施

5. **跨模块依赖**：`Application.Contracts` 层引用的 IServices 接口在测试中也需注册
   ```csharp
   // 如果测试的服务依赖其他模块的 IService
   services.AddScoped<IWorkOrderService>(sp => new WorkOrderService());
   ```

6. **服务注入方式**：
   - 生产环境：`AddCoreAssembly("Lzq.")` 自动扫描
   - 测试环境：需手动注册或用 `AddCoreAssembly`
   ```csharp
   var services = new ServiceCollection();
   services.AddCoreAssembly("Lzq.");  // 自动扫描注册
   services.AddMapster();
   ```

### 接口返回类型必须强类型

```csharp
// ✅ 正确
Task<ApiResult<PagedResponse<LineDto>>> PageAsync(LinePageQuery query);

// ❌ 错误 - 非强类型
Task<ApiResult> PageAsync(LinePageQuery query);
```

### 测试中的 GetData 调用

```csharp
// ✅ 正确 - 使用 result.Data
result.Data.Should().NotBeNull();
result.Data.Total.Should().Be(2);

// ❌ 错误 - 使用 GetData<T>
result.GetData<PagedResponse<LineDto>>()!.Total.Should().Be(2);
```

---

## 命名空间速查表

| 类型 | 正确命名空间 | 常见错误 |
|---|---|---|
| `BaseFullEntity` | `Lzq.Extensions.SqlSugar.Entities` | ~~Lzq.Core.Entities~~ |
| `BaseEntity` (不存在) | 使用 `BaseFullEntity` 代替 | ~~BaseEntity~~ |
| `BaseSeedData<T>` | `Lzq.Extensions.SqlSugar.Entities` | ~~Lzq.Core.SeedDatas~~ |
| `ISqlSugarRepository<T>` | `Lzq.Extensions.SqlSugar.Repository` | ~~Lzq.Core.Repositories~~ |
| `SqlSugarRepository<T>` | `Lzq.Extensions.SqlSugar.Repository` | ~~Lzq.Core.Repositories~~ |
| `ITransientDependency` | `Microsoft.Extensions.DependencyInjection` | ~~Lzq.Core.Dependency~~ |
| `ServiceBase` | `Microsoft.AspNetCore.Builder` | ~~Lzq.Core.Services~~ |
| `[FromBody]` | `Microsoft.AspNetCore.Mvc` | 容易遗漏 |
| `[FromQuery]` | `Microsoft.AspNetCore.Mvc` | 容易遗漏 |
| `PagedRequest/PagedResponse` | `Lzq.Core.Models` | ~~Lzq.Core.Pagination~~ |
| `UserFriendlyException` | `System` (global using) | ~~Lzq.Core.Exceptions~~ |
| `FluentValidation` | `FluentValidation` | ~~Lzq.Core.FluentValidation~~ |
| `AbstractValidator<T>` | `FluentValidation` | 同上 |
| `AddCoreAssembly` | `Lzq.Extensions.SqlSugar.Repository` | ~~Microsoft.AspNetCore.Builder~~ |
| `AddCoreAutoInject` | `Lzq.Extensions.SqlSugar.Repository` | ~~Lzq.Core.Exceptions~~ |

> ⚠️ **重要发现**：`Lzq.Extensions.SqlSugar.Repository` 同时替代了以下旧命名空间：
> - `Microsoft.AspNetCore.Builder`（ServiceBase 相关）
> - `Lzq.Core.Exceptions`（UserFriendlyException 虽然实际在 System，但 AddCoreAutoInject 等扩展方法在此）
> - `Lzq.Core.Repositories`（仓储接口）

---

## 架构问题处理规则

🚨 发现架构问题时：记录 → 说明原因 → 等待决策 → 执行，**不得自行修改**。
