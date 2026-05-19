# 10 - 模块开发参考

> 来源: lzq-module-development | 状态: ✅ 已根据 problems 修正

## 模块依赖矩阵

| 层 | 允许引用 |
|---|---|
| Domain | 本模块的 Domain（实体、仓储接口）|
| Application.Contracts | 本模块的 Domain、Lzq 扩展包 |
| Application | 本模块的 Domain + Contracts、其他模块的 Contracts |

## 动态 API 注册条件

1. 服务类继承 `ServiceBase`
2. 构造函数传入基础路由
3. 公共方法标记 `[RoutePattern]`
4. 在 `Program.cs` 调用 `AddCoreMinimalAPIs()` + `MapMasaMinimalAPIs()`

## 自动 DI 注册

实现以下接口的类会自动注册：
- `ITransientDependency` → Transient
- `IScopedDependency` → Scoped
- `ISingletonDependency` → Singleton

## 跨模块调用架构

### 依赖方向（不允许）
```
ModuleA.Application → ModuleB.Domain  ❌ 禁止
```

### 依赖方向（允许）
```
ModuleA.Application → ModuleB.Application.Contracts  ✅ 通过接口
```

### 引用服务模式

```csharp
// 1. Contracts 层定义接口
public interface IReferenceDataService
{
    Task<SomeDto?> GetByIdAsync(long id);
}

// 2. Application 层实现（内部引用其他 Domain）
public class ReferenceDataService : IReferenceDataService, ITransientDependency
{
    // 可以引用目标模块的 Domain 仓储
}

// 3. Application.csproj 添加对目标 Domain 的引用
<ProjectReference Include="..\..\BaseData\Lzq.BaseData.Domain\..." />
```

## Lzq.Extensions 发布流程

1. 修改 `Lzq.Extensions\Directory.Build.props` 版本号
2. 执行 `build.bat` 打包
3. 更新 `WorkBuddy\code\Directory.Build.props` 版本号
4. 构建验证
5. 记录到 `problems/`
