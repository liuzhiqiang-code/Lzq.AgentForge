# 01 — 核心框架层改进

> **关联**: [README.md](./README.md) | **优先级覆盖**: P0-1~P0-3, P1-1~P1-5, P2-1, P2-3, P2-5

---

## 1. IRepository\<T\> 抽象层 (P0-1 🔴)

### 当前问题

Application 层 Service 直接依赖 `ISqlSugarRepository<T>`，与 SqlSugar ORM 强耦合：

```csharp
// ❌ 当前写法：Application 层直接依赖 SqlSugar 具体类型
private ISqlSugarRepository<FactoryEntity> Repo =>
    GetRequiredService<ISqlSugarRepository<FactoryEntity>>();
```

**危害**：Application 层无法脱离 SqlSugar 做单元测试；`ISqlSugarRepository<T>` 暴露了 SqlSugar 专有 API（如 `AsQueryable()`），违背依赖倒置原则。

### 改进方案

在 `Lzq.Core` 中引入 `IRepository<T>` 抽象接口：

```csharp
// Lzq.Core/Repository/IRepository.cs
namespace Lzq.Core.Repository;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(long id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate);
    Task<(List<T> Items, int Total)> GetPagedAsync(
        Expression<Func<T, bool>>? predicate, int page, int pageSize,
        Expression<Func<T, object>>? orderBy = null, bool descending = true);
    Task<long> InsertAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(long id);
    Task<bool> DeleteAsync(Expression<Func<T, bool>> predicate);
    Task InsertRangeAsync(List<T> entities);
    Task UpdateRangeAsync(List<T> entities);
}
```

**SqlSugar 适配器** (`Lzq.Extensions.SqlSugar`)：

```csharp
public class SqlSugarRepositoryAdapter<T> : IRepository<T> where T : class, new()
{
    private readonly SqlSugarRepository<T> _inner;
    public SqlSugarRepositoryAdapter(ISqlSugarClient client) => _inner = new SqlSugarRepository<T>(client);

    public Task<T?> GetByIdAsync(long id) => _inner.GetByIdAsync(id);
    public Task<List<T>> GetAllAsync() => _inner.GetListAsync();
    public async Task<(List<T>, int)> GetPagedAsync(
        Expression<Func<T, bool>>? predicate, int page, int pageSize,
        Expression<Func<T, object>>? orderBy = null, bool descending = true)
    {
        RefAsync<int> total = 0;
        var query = _inner.AsQueryable().WhereIF(predicate != null, predicate);
        if (orderBy != null)
            query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
        var items = await query.ToPageListAsync(page, pageSize, total);
        return (items, total);
    }
    // ... 其余方法
}
```

**迁移策略**：阶段一新增接口和适配器 → 阶段二逐模块迁移 Service → 阶段三标记直接使用 `ISqlSugarRepository<T>` 为废弃。

---

## 2. 构造函数注入替代 Service Locator (P0-2 🔴)

### 当前问题

属性注入（Service Locator 反模式）：

```csharp
// ❌ 当前：每个属性访问都是 GetRequiredService 调用
private ISqlSugarRepository<FactoryEntity> Repo
    => GetRequiredService<ISqlSugarRepository<FactoryEntity>>();
private ILogger<FactoryService> Logger
    => GetRequiredService<ILogger<FactoryService>>();
```

**危害**：依赖项隐式声明，单元测试必须配置 DI 容器；每个访问都有 DI 解析开销；违反显式依赖原则。

### 改进方案

```csharp
// ✅ 改进：构造函数显式注入
public class FactoryService : ServiceBase, IFactoryService
{
    private readonly IRepository<FactoryEntity> _repo;
    private readonly ILogger<FactoryService> _logger;
    private readonly ICurrentUser _currentUser;

    public FactoryService(
        IRepository<FactoryEntity> repo,
        ILogger<FactoryService> logger,
        ICurrentUser currentUser)
        : base("/api/v1/basedata/factory")
    {
        _repo = repo;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<ApiResult<FactoryDto>> GetAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null)
            throw new UserFriendlyException("工厂不存在");
        return ApiResult.Success(entity.Map<FactoryDto>());
    }
}
```

> ⚠️ 需要确认 `ServiceBase` 路由注册机制是否兼容构造函数注入。当前 `ServiceBase(baseRoute)` 构造函数模式需要保留。

---

## 3. 审计字段 AOP 自动填充 (P0-3 🔴)

### 当前问题

`BaseFullEntity` 定义了 `Creator`、`CreationTime`、`Modifier`、`ModificationTime` 审计字段，但需手动填充。SqlSugar 虽有 AOP 机制，但缺乏统一封装。

### 改进方案

```csharp
// Lzq.Extensions.SqlSugar/Aop/AuditAop.cs
public static class AuditAopConfig
{
    public static void Configure(ISqlSugarClient db, ICurrentUser currentUser)
    {
        // 插入时自动填充
        db.Aop.DataExecuting = (oldValue, entityInfo) =>
        {
            if (entityInfo.OperationType == DataFilterType.InsertByObject)
            {
                if (entityInfo.Entity is IBaseFullEntity entity)
                {
                    entity.CreationTime = DateTime.Now;
                    entity.Creator = currentUser.UserId ?? 0;
                    entity.ModificationTime = DateTime.Now;
                    entity.Modifier = currentUser.UserId ?? 0;
                }
            }
            if (entityInfo.OperationType == DataFilterType.UpdateByObject)
            {
                if (entityInfo.Entity is IBaseFullEntity entity)
                {
                    entity.ModificationTime = DateTime.Now;
                    entity.Modifier = currentUser.UserId ?? 0;
                }
            }
        };
    }
}
```

---

## 4. IUnitOfWork 可注入接口 (P1-1 🟡)

### 当前问题

`[UnitOfWork]` 是一个 AOP 特性，无法在代码中显式控制事务边界：

```csharp
// ❌ 当前：只能通过特性声明事务
[UnitOfWork(isolationLevel: IsolationLevel.ReadCommitted)]
public async Task<ApiResult> CreateAsync([FromBody] CreateCommand cmd) { ... }
```

### 改进方案

```csharp
// Lzq.Core/Transactions/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}

// 使用方式
public class FactoryService : ServiceBase, IFactoryService
{
    private readonly IUnitOfWorkManager _uowManager;

    public async Task<ApiResult> ComplexOperationAsync(ComplexCommand cmd)
    {
        using var uow = await _uowManager.BeginAsync(IsolationLevel.ReadCommitted);
        try
        {
            await _repo1.InsertAsync(entity1);
            await _repo2.InsertAsync(entity2);
            await uow.CommitAsync();
            return ApiResult.Success();
        }
        catch
        {
            await uow.RollbackAsync();
            throw;
        }
    }
}
```

> 保留 `[UnitOfWork]` 特性作为便捷方式，新增 `IUnitOfWorkManager` 作为可注入接口。

---

## 5. 统一异常处理 (P1-2 🟡)

### 当前问题

错误响应分散：`UserFriendlyException`、`MasaValidatorException`、手动 `ApiResult.Fail()` 三种模式并存，无统一 ProblemDetails (RFC 7807) 格式。

### 改进方案

```csharp
// 统一异常处理中间件
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (UserFriendlyException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Type = "https://errors.example.com/business-error",
                Title = "业务错误",
                Detail = ex.Message,
                Status = 400,
                Instance = context.Request.Path
            });
        }
    }
}

// 业务异常编码体系
public class CodedException : UserFriendlyException
{
    public string ErrorCode { get; }  // "FACTORY.001"
    public string ErrorCategory { get; }  // "VALIDATION" / "BUSINESS" / "RESOURCE"
    public string? Suggestion { get; }  // 建议操作
}
```

---

## 6. ExternalHttpApi 弹性策略 (P1-4 🟡)

### 当前问题

`IExternalHttpApi` 无内置熔断、重试、超时策略，调用外部 API 缺乏弹性保护。

### 改进方案

```csharp
// 配置中添加弹性策略
builder.Services.AddExternalHttpApis(builder.Configuration, options =>
{
    options.AddPolicy("UserApi", policy => policy
        .WithRetry(3, TimeSpan.FromSeconds(1))
        .WithCircuitBreaker(5, TimeSpan.FromSeconds(30))
        .WithTimeout(TimeSpan.FromSeconds(15)));
});

// 或在接口定义时声明
[RetryPolicy(retryCount: 3, delayMs: 1000)]
[CircuitBreaker(failureThreshold: 5, breakDurationSeconds: 30)]
public interface IUserApi : IExternalHttpApi { ... }
```

---

## 7. Feature Flags 功能开关 (P2-3 🔵)

### 改进方案

```csharp
// Lzq.Core/Features/IFeatureManager.cs
public interface IFeatureManager
{
    Task<bool> IsEnabledAsync(string feature, long? tenantId = null);
}

// 使用方式
[FeatureGate("new-checkout-flow")]
public async Task<ApiResult> CheckoutAsync(CheckoutCommand cmd)
{
    if (await _featureManager.IsEnabledAsync("email-notification"))
        await _emailService.SendOrderConfirmationAsync(order);
}
```

---

## 8. 缓存抽象增强 (P2-5 🔵)

### 改进方案

在现有 `ILzqRedisClient` 基础上增加缓存失效策略接口：

```csharp
// 缓存标签批量失效
public interface ICacheTagInvalidator
{
    Task InvalidateByTagAsync(string tag);  // 失效所有带此标签的缓存
}

// 使用
await _redis.SetAsync("factory:123", data, tags: new[] { "factory", "factory:123" });
// 更新工厂时，只需
await _cacheTagInvalidator.InvalidateByTagAsync("factory");
// 所有 factory 相关的缓存全部失效
```
