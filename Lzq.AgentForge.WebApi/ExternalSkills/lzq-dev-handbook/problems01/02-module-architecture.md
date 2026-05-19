# 02 — 模块架构改进

> **关联**: [README.md](./README.md) | **优先级覆盖**: P1-3, P2-1, P2-2, P2-4

---

## 1. 模块清单与显式注册 (P1-3 🟡)

### 当前问题

模块通过程序集扫描自动发现，无显式注册机制。问题在于：
- 无法直观了解哪些模块已加载
- 无法控制模块加载顺序
- 模块自身的配置、种子数据、健康检查零散分布

### 改进方案

引入 `IModule` 接口，每个模块显式声明其构成：

```csharp
// Lzq.Core/Modules/IModule.cs
public interface IModule
{
    string Name { get; }
    string Version { get; }
    Assembly DomainAssembly { get; }
    Assembly ApplicationAssembly { get; }
    Assembly ContractsAssembly { get; }

    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    void OnApplicationStartup(IApplicationBuilder app);
}

// 模块实现示例
public class BaseDataModule : IModule
{
    public string Name => "Lzq.BaseData";
    public string Version => "1.0.0";
    public Assembly DomainAssembly => typeof(FactoryEntity).Assembly;
    public Assembly ApplicationAssembly => typeof(FactoryService).Assembly;
    public Assembly ContractsAssembly => typeof(IFactoryService).Assembly;

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // 模块特有的服务注册、种子数据注册
    }

    public void OnApplicationStartup(IApplicationBuilder app)
    {
        // 模块启动时的初始化工作
    }
}

// Program.cs — 显式注册
builder.AddModule<BaseDataModule>();
builder.AddModule<EquipmentModule>();
builder.AddModule<QAModule>();
builder.AddModule<WorkOrderModule>();
```

### 模块清单可视化

```csharp
// 自动生成模块清单 API
[HttpGet("api/v1/system/modules")]
public IActionResult GetModules()
{
    var modules = app.Services.GetServices<IModule>()
        .Select(m => new { m.Name, m.Version, Status = "Running" });
    return Ok(modules);
}
```

---

## 2. 领域事件与集成事件分层 (P2-1 🔵)

### 当前问题

当前 EventBus 主要处理进程内事件（`BaseLocalEvent`）和集成事件（`BaseIntegrationEvent`），但缺少**领域事件**概念。领域事件应在同一聚合内的多个实体间传递，不应直接发布为集成事件。

### 改进方案

```
层次模型：

领域事件 (Domain Event)
  ↓ 聚合内传播，事务一致性
应用事件 (Application Event)
  ↓ 跨聚合传播，同一进程
集成事件 (Integration Event)
  ↓ 跨服务传播，最终一致性
```

```csharp
// 领域事件基类
public abstract record BaseDomainEvent : INotification
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

// 聚合根发布领域事件
public class WorkOrderAggregate
{
    private readonly List<BaseDomainEvent> _domainEvents = new();

    public void Dispatch()
    {
        var @event = new WorkOrderDispatchedEvent(Id, LineId);
        _domainEvents.Add(@event);
    }

    public IReadOnlyList<BaseDomainEvent> GetDomainEvents() => _domainEvents;
}

// 领域事件处理器（进程内）
public class WorkOrderDispatchedHandler : INotificationHandler<WorkOrderDispatchedEvent>
{
    public Task Handle(WorkOrderDispatchedEvent notification, CancellationToken ct)
    {
        // 更新设备状态、发送通知等
        return Task.CompletedTask;
    }
}
```

### 领域事件 → 集成事件映射

```csharp
// 在 Application 层将领域事件映射为集成事件
public class WorkOrderIntegrationEventMapper : INotificationHandler<WorkOrderDispatchedEvent>
{
    private readonly IIntegrationEventPublisher _publisher;

    public async Task Handle(WorkOrderDispatchedEvent domainEvent, CancellationToken ct)
    {
        var integrationEvent = new WorkOrderDispatchedIntegrationEvent(
            domainEvent.WorkOrderId, domainEvent.LineId);
        await _publisher.PublishAsync(integrationEvent, ct);
    }
}
```

---

## 3. 多租户策略文档化 (P2-4 🔵)

### 当前状态

实体上有 `[Tenant("AgentForge")]` 特性，数据库配置中有 `ConfigId`，但多租户的整体策略缺乏文档。

### 建议完善

```markdown
## 多租户架构决策

| 决策项 | 当前方案 | 说明 |
|--------|---------|------|
| 隔离策略 | 数据库分离 | 每个租户独立数据库实例 |
| 租户识别 | Tenant 特性 + ConfigId | 通过 `[Tenant("name")]` 将实体绑定到特定数据库配置 |
| 数据共享 | 无 | 所有实体均绑定租户 |
| 租户切换 | 静态绑定 | 实体编译时确定所属租户 |

### 未来演进路径
1. **租户上下文**：引入 `ITenantContext`，运行时动态解析租户
2. **混合策略**：支持部分表共享、部分表隔离
3. **租户管理 API**：租户创建、配置、数据库初始化自动化
```

---

## 4. 跨模块通信标准化 (P1-3 补充)

### 当前状态与不足

已有两种跨模块调用模式（直调 Contracts 服务 / 创建 ReferenceDataService），但缺少标准化的模块间通信协议。

### 建议补充

```csharp
// 模式三：模块间查询对象 (Module Query Object)
public record FactoryInfoQuery(long FactoryId) : IModuleQuery<FactoryInfoDto>;

// 查询总线自动路由到正确的模块
public interface IModuleQueryBus
{
    Task<TResult> QueryAsync<TResult>(IModuleQuery<TResult> query);
}

// 使用
var factory = await _queryBus.QueryAsync(new FactoryInfoQuery(factoryId));
```

这样模块间调用完全通过消息对象解耦，调用方无需知道目标模块的具体服务接口。
