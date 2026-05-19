# Lzq.Core 核心基础库

`Lzq.Core` 提供 Web API 开发的基础设施：`ServiceBase`、`ApiResult`、依赖注入、MinimalAPI 支持等。

---

## ServiceBase 服务基类

```csharp
using Microsoft.AspNetCore.Builder;  // ✅ ServiceBase 在此命名空间

public class XxxService : ServiceBase , IXXXService
{
    public XxxService() : base("/api/v1/module/entity") { }
    
    // 使用属性注入获取依赖
    private ISqlSugarRepository<XxxEntity> Repo => GetRequiredService<ISqlSugarRepository<XxxEntity>>();
    private ILogger<XxxService> Logger => GetRequiredService<ILogger<XxxService>>();
    private ICurrentUser CurrentUser => GetRequiredService<ICurrentUser>();
}
```

---

## ApiResult 统一响应

| 场景 | 用法 |
|---|---|
| 成功，无数据 | `return ApiResult.Success();` |
| 成功，带数据 | `return ApiResult.Success(data);` |
| 业务错误 | `throw new UserFriendlyException("消息");` |
| 验证错误 | `throw new MasaValidatorException(...);` |
| 未找到 | `return ApiResult.Fail("未找到", 404);` |

> **注意**：`UserFriendlyException` 在 `System` 命名空间下（全局 using 已包含）。

---

## Service 方法必须实现接口 🚨

```csharp
// ✅ 正确：Service 必须实现对应接口
public class WorkOrderService : ServiceBase, IWorkOrderService
{
    // 方法签名必须与接口完全匹配
    public async Task<ApiResult<PagedResponse<WorkOrderViewDto>>> PageAsync(WorkOrderPageQuery query)
    {
        ...
    }
}
```

---

## ApiResult<T> 返回类型匹配

| 接口定义 | 实现签名 |
|---|---|
| `Task<ApiResult<PagedResponse<TDto>>>` | `Task<ApiResult<PagedResponse<TDto>>>` |
| `Task<ApiResult<TDto>>` | `Task<ApiResult<TDto>>` |
| `Task<ApiResult<long>>` | `Task<ApiResult<long>>` |
| `Task<ApiResult<List<TDto>>>` | `Task<ApiResult<List<TDto>>>` |

---

## Http 相关特性 using

| 特性/类型 | 命名空间 |
|---|---|
| `ServiceBase` | `Microsoft.AspNetCore.Builder` |
| `[FromBody]` | `Microsoft.AspNetCore.Mvc` |
| `[FromQuery]` | `Microsoft.AspNetCore.Mvc` |
| `[FromRoute]` | `Microsoft.AspNetCore.Mvc` |
| `PagedRequest` / `PagedResponse<T>` | `Lzq.Core.Models` |

---

## API 端点注册

```csharp
// 方法特性
[OpenApiTag("module/entity")]
[OpenApiOperation("操作摘要", "详细描述")]
[RoutePattern(pattern: "action", true)]  // true = POST 方法

// route 示例：
// "page" → POST /api/v1/module/entity/page
// "create" → POST /api/v1/module/entity/create
// "{id}" → GET /api/v1/module/entity/123
// "delete/{id}" → DELETE /api/v1/module/entity/delete/123
```

---

## Service 实现方法必须添加参数绑定特性 🚨

> **强制规则**：Service 实现类中暴露为 HTTP API 端点的方法（即有 `[RoutePattern]` 特性的方法），必须根据请求类型为复杂参数添加 `[FromBody]`，为简单参数添加 `[FromQuery]`/`[FromRoute]`。**接口层（IService）绝对不能添加这些绑定特性。**

### 规则说明

| 参数类型 | 绑定特性 | 适用场景 |
|---|---|---|
| 复杂类型（Command/DTO/Query/List） | `[FromBody]` | POST/PUT 请求，从请求体读取 |
| 简单类型（int/long/string/DateTime/bool） | `[FromQuery]` | GET 请求，从查询字符串读取 |
| 路由参数（`{id}`） | `[FromRoute]` | 从 URL 路径段提取（可省略，ASP.NET Core 隐式绑定） |
| 简单类型可选参数（`?lineId=null`） | `[FromQuery]`（推荐显式标注） | GET 请求可选参数 |

### 正确示例

```csharp
// ✅ 接口层：不添加任何绑定特性
public interface IWorkOrderService
{
    Task<ApiResult<PagedResponse<WorkOrderViewDto>>> PageAsync(WorkOrderPageQuery query);
    Task<ApiResult<bool>> CreateAsync(WorkOrderCreateCommand command);
    Task<ApiResult<WorkOrderViewDto>> GetAsync(long id);
    Task<ApiResult<List<WorkOrderViewDto>>> GetByLineIdAsync(long lineId);
}
```

```csharp
// ✅ Service 实现层：根据 HTTP 方法添加对应绑定特性
public class WorkOrderService : ServiceBase, IWorkOrderService
{
    // POST + 复杂类型 → [FromBody]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<WorkOrderViewDto>>> PageAsync(
        [FromBody] WorkOrderPageQuery query) { ... }

    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<bool>> CreateAsync(
        [FromBody] WorkOrderCreateCommand command) { ... }

    // GET + 路由参数 → 隐式 [FromRoute]
    [RoutePattern(pattern: "{id}", false)]
    public async Task<ApiResult<WorkOrderViewDto>> GetAsync(long id) { ... }

    // GET + 简单类型 → [FromQuery]
    [RoutePattern(pattern: "by-line/{lineId}", false)]
    public async Task<ApiResult<List<WorkOrderViewDto>>> GetByLineIdAsync(
        [FromQuery] long lineId) { ... }
}
```

### 错误示例

```csharp
// ❌ 错误：接口层添加了 [FromBody]（绝对禁止！）
public interface IWorkOrderService
{
    Task<ApiResult<bool>> CreateAsync([FromBody] WorkOrderCreateCommand command); // ❌
}

// ❌ 错误：Service 实现层复杂类型缺少 [FromBody]
[RoutePattern(pattern: "create", true)]
public async Task<ApiResult<bool>> CreateAsync(WorkOrderCreateCommand command) // ❌
{ ... }
```

### 判断依据速查

| RoutePattern(pattern, isPost) | HTTP 方法 | 复杂类型参数 | 简单类型参数 |
|---|---|---|---|
| `RoutePattern("action", true)` | POST | `[FromBody]` ✅ | — |
| `RoutePattern("action", false)` | GET | — | `[FromQuery]` ✅ |
| `RoutePattern("{id}", false)` | GET | — | 隐式 `[FromRoute]` |
| 无 `[RoutePattern]` | 内部方法 | 不需要 | 不需要 |

### 注意事项

1. **只在有 `[RoutePattern]` 的 Service 实现方法上加绑定特性**，内部方法（无 RoutePattern）不需要加。
2. **接口永远是干净的**——绑定特性是实现细节，不属于契约的一部分。
3. `[FromRoute]` 在 ASP.NET Core 中对简单类型路由参数是隐式的，通常可省略，但显式标注可提高可读性。
4. 批量操作如 `List<long> ids` 应标注 `[FromBody]`，因为复杂集合类型无法从查询字符串绑定。

---

## C# 可空引用类型规范 🚨

项目启用了 `<Nullable>enable</Nullable>`，必须遵循可空引用类型规范：

| 场景 | 写法 | 说明 |
|---|---|---|
| **必填字段** `string` | `public string Name { get; set; } = string.Empty;` | 必须初始化，避免 null 警告 |
| **可选字段** `string?` | `public string? Description { get; set; }` | 直接声明即可，无需默认值 |
| **值类型可选** `int?` | `public int? Sort { get; set; }` | 使用 nullable 值类型 |

```csharp
// ✅ 正确示例
public class ProcessEntity : BaseFullEntity
{
    [SugarColumn(ColumnName = "name", Length = 100)]
    public string Name { get; set; } = string.Empty;  // 必填，初始化
    
    [SugarColumn(ColumnName = "description", Length = 500, IsNullable = true)]
    public string? Description { get; set; }  // 可选，可空
    
    [SugarColumn(ColumnName = "sort")]
    public int? Sort { get; set; }  // 可选值类型
}
```

> ⚠️ **注意**：在 SqlSugar CodeFirst 中，可空属性必须显式添加 `[SugarColumn(IsNullable = true)]`，否则数据库列会是 NOT NULL。

---

## 架构问题处理规则

🚨 发现架构问题时：记录 → 说明原因 → 等待决策 → 执行，**不得自行修改**。
