# Lzq 框架已知问题汇总

> **维护日期**: 2026-05-14 | **基于**: Lzq.Extensions 0.1.36 | **关联**: SKILL.md §16

本文档记录了 Lzq 框架在实际项目开发中暴露的所有已知问题、根因分析、修复方案及预防措施。每个问题按发现轮次组织，累计修复 **168 个文件/位置**。

---

## 目录

- [第一轮：实体 Entity 命名空间错误](#第一轮实体-entity-命名空间错误)
- [第二轮：Repository 实现类命名空间](#第二轮repository-实现类命名空间)
- [第三轮：PageQuery/Service 文件分页命名空间](#第三轮pagequeryservice-文件分页命名空间)
- [第四轮：IServices 接口 [FromBody] 特性](#第四轮iservices-接口-frombody-特性)
- [第五轮：ServiceBase 命名空间](#第五轮servicebase-命名空间)
- [第六轮：[FromBody] Mvc 命名空间缺失](#第六轮frombody-mvc-命名空间缺失)
- [第七轮：CreatedTime → CreationTime 属性名](#第七轮createdtime--creationtime-属性名)
- [第八轮：BaseData.Tests 测试文件](#第八轮basedatatest-测试文件)
- [第九轮：ApiResult 扩展方法与 Seed 重载](#第九轮apiresult-扩展方法与-seed-重载)
- [第十轮：AI 模块 Vue 文件中文乱码（UTF-8 编码损坏）](#第十轮ai-模块-vue-文件中文乱码utf-8-编码损坏)
- [Lzq.MES 编译问题](#lzqmes-编译问题)
- [WorkOrder 模块耦合问题](#workorder-模块耦合问题)
- [Equipment 模块架构问题](#equipment-模块架构问题)
- [根因分析与系统性改进建议](#根因分析与系统性改进建议)

---

## 第一轮：实体 Entity 命名空间错误

**发现时间**: 2026-05-13 22:10 | **严重程度**: 🔴 阻塞编译 | **影响**: 13 个文件

### 问题描述

所有实体类使用了不存在的命名空间 `using Lzq.Core.Entities;` 来引用 `BaseFullEntity`。

### 错误写法

```csharp
using Lzq.Core.Entities;  // ❌ 不存在此命名空间

public class FactoryEntity : BaseFullEntity { }
```

### 正确写法

```csharp
using Lzq.Extensions.SqlSugar.Entities;  // ✅ BaseFullEntity 真实位置

public class FactoryEntity : BaseFullEntity { }
```

### 根因分析

- **Skill 文档错误**: `lzq-module-development` skill 的实体示例中使用了错误命名空间
- **Lzq.Core.Entities** 命名空间中不包含 `BaseFullEntity` 类
- 该类型实际定义在 `Lzq.Extensions.SqlSugar` NuGet 包的 `Entities` 命名空间下

### 受影响文件 (13 个)

| 模块 | 文件 |
|------|------|
| BaseData | FactoryEntity.cs, LineEntity.cs, ProcessEntity.cs, WorkshopEntity.cs |
| Dashboard | DashboardConfigEntity.cs |
| Equipment | EquipmentEntity.cs, InspectionEntity.cs, MaintenanceEntity.cs |
| QA | DefectRecordEntity.cs, QCOrderEntity.cs, QCOrderItemEntity.cs |
| WorkOrder | WorkOrderEntity.cs, WorkReportEntity.cs |

### 预防措施

- ✅ SKILL.md §4.2 步骤 2 已使用正确命名空间
- ✅ §15 命名空间速查表首行标注
- ✅ AI 审查清单包含此项检查

---

## 第二轮：Repository 实现类命名空间

**发现时间**: 2026-05-13 22:26 | **严重程度**: 🔴 阻塞编译 | **影响**: 24 个文件（接口 11 + 实现 13）

### 问题描述

IRepository 接口和 Repository 实现类使用了两个不存在的命名空间：
- `using Lzq.Core.Dependency;` ❌
- `using Lzq.Core.Repositories;` ❌

### 错误写法

```csharp
using Lzq.Core.Dependency;    // ❌ 不存在
using Lzq.Core.Repositories;  // ❌ 不存在

public interface IFactoryRepository : ISqlSugarRepository<FactoryEntity>, ITransientDependency { }

public class FactoryRepository : SqlSugarRepository<FactoryEntity>, IFactoryRepository { }
```

### 正确写法

```csharp
using Lzq.Extensions.SqlSugar.Repository;         // ✅ ISqlSugarRepository<T>, SqlSugarRepository<T>
using Microsoft.Extensions.DependencyInjection;   // ✅ ITransientDependency

public interface IFactoryRepository : ISqlSugarRepository<FactoryEntity>, ITransientDependency { }

public class FactoryRepository : SqlSugarRepository<FactoryEntity>, IFactoryRepository { }
```

### 根因分析

- `ISqlSugarRepository<T>` 和 `SqlSugarRepository<T>` 定义在 `Lzq.Extensions.SqlSugar.Repository` 命名空间
- `ITransientDependency` 是 .NET 原生 `Microsoft.Extensions.DependencyInjection` 中的类型
- Skill 文档错误地将这些类型归入了 `Lzq.Core.*` 命名空间

### 受影响文件

**IRepository 接口 (11 个)**: IFactoryRepository, ILineRepository, IProcessRepository, IWorkshopRepository, IDashboardConfigRepository, IEquipmentRepository, IInspectionRepository, IMaintenanceRepository, IDefectRecordRepository, IQCOrderRepository, IWorkOrderRepository, IWorkReportRepository

**Repository 实现 (13 个)**: FactoryRepository, LineRepository, ProcessRepository, WorkshopRepository, DashboardConfigRepository, EquipmentRepository, InspectionRepository, MaintenanceRepository, DefectRecordRepository, QCOrderRepository, WorkReportRepository, ServiceTestBase, TestDbContext

### 验证命令

```bash
grep -rn "using Lzq.Core.Repositories" /d/gitee/WorkBuddy/code --include="*.cs"
# 空结果 = 全部修复
```

---

## 第三轮：PageQuery/Service 文件分页命名空间

**发现时间**: 2026-05-13 22:38 | **严重程度**: 🔴 阻塞编译 | **影响**: 21 个文件

### 问题描述

大量文件使用了不存在的 `using Lzq.Core.Pagination;` 来引用 `PagedRequest` 和 `PagedResponse<T>`。

### 错误写法

```csharp
using Lzq.Core.Pagination;  // ❌ 不存在此命名空间

public record FactoryPageQuery : PagedRequest { }
```

### 正确写法

```csharp
using Lzq.Core.Models;  // ✅ PagedRequest / PagedResponse<T> 真实位置

public record FactoryPageQuery : PagedRequest { }
```

### 受影响文件 (21 个)

- **Queries (4 个)**: FactoryQueries, LineQueries, ProcessQueries, WorkshopQueries — 需要**改为** `Lzq.Core.Models`
- **Service 接口 (6 个)**: IEquipmentService, IInspectionService, IMaintenanceService, IDefectRecordService, IQCOrderService, IWorkOrderService — 需要**删除**错误 using
- **Service 实现 (11 个)**: 全部 11 个 Service 实现类 — 需要**删除**错误 using

---

## 第四轮：IServices 接口 [FromBody] 特性

**发现时间**: 2026-05-13 22:45 | **严重程度**: 🟡 警告 | **影响**: 52 处

### 问题描述

IServices 接口定义中的方法参数添加了 `[FromBody]` 特性，但这是不必要的。根据规范，`[FromBody]` 只需在 Service 实现类中添加。

### 错误写法

```csharp
// IServices 接口定义
public interface IFactoryService
{
    Task<ApiResult> PageAsync([FromBody] FactoryPageQuery query);  // ❌ 接口不需要
    Task<ApiResult> CreateAsync([FromBody] FactoryCreateCommand cmd); // ❌
}
```

### 正确写法

```csharp
// IServices 接口定义 — 无 [FromBody]
public interface IFactoryService
{
    Task<ApiResult> PageAsync(FactoryPageQuery query);       // ✅
    Task<ApiResult> CreateAsync(FactoryCreateCommand cmd);    // ✅
}

// Service 实现类 — 保留 [FromBody]
public class FactoryService : ServiceBase, IFactoryService
{
    public async Task<ApiResult> PageAsync([FromBody] FactoryPageQuery query) { ... }  // ✅
    public async Task<ApiResult> CreateAsync([FromBody] FactoryCreateCommand cmd) { ... } // ✅
}
```

### 设计原理

- 接口定义的是契约，不应与特定传输协议绑定
- `[FromBody]` 仅在 HTTP 端点绑定时生效，应由实现层负责
- 如果未来需要通过 gRPC 或消息队列调用同一接口，接口中的 `[FromBody]` 会成为污染

---

## 第五轮：ServiceBase 命名空间

**发现时间**: 2026-05-13 22:53 | **严重程度**: 🔴 阻塞编译 | **影响**: 12 个文件

### 问题描述

所有 Service 实现类使用了 `using Lzq.Core.Services;` 引用 `ServiceBase`，但该类型实际位于 `Microsoft.AspNetCore.Builder`。

### 错误写法

```csharp
using Lzq.Core.Services;  // ❌ 不存在

public class FactoryService : ServiceBase, IFactoryService { }
```

### 正确写法

```csharp
using Microsoft.AspNetCore.Builder;  // ✅ ServiceBase 真实位置

public class FactoryService : ServiceBase, IFactoryService { }
```

### 根因分析

`ServiceBase` 是 ASP.NET Core Minimal API 基础设施的一部分，被放在了 `Microsoft.AspNetCore.Builder` 命名空间中以符合 .NET 生态惯例。但开发者可能误以为所有 Lzq 框架类型都在 `Lzq.Core.*` 下。

---

## 第六轮：[FromBody] Mvc 命名空间缺失

**发现时间**: 2026-05-13 22:57 | **严重程度**: 🔴 阻塞编译 | **影响**: 12 个文件

### 问题描述

Service 实现类中使用了 `[FromBody]` 特性，但缺少 `using Microsoft.AspNetCore.Mvc;`。

### 错误写法

```csharp
using Microsoft.AspNetCore.Builder;  // 只有 ServiceBase

public class FactoryService : ServiceBase, IFactoryService
{
    [FromBody]  // ❌ 编译错误：找不到 FromBodyAttribute
    public async Task<ApiResult> CreateAsync(FactoryCreateCommand cmd) { }
}
```

### 正确写法

```csharp
using Microsoft.AspNetCore.Builder;   // ServiceBase
using Microsoft.AspNetCore.Mvc;       // [FromBody], [FromQuery], [FromRoute]

public class FactoryService : ServiceBase, IFactoryService
{
    [FromBody]  // ✅ 正确
    public async Task<ApiResult> CreateAsync(FactoryCreateCommand cmd) { }
}
```

---

## 第七轮：CreatedTime → CreationTime 属性名

**发现时间**: 2026-05-13 23:05 | **严重程度**: 🔴 运行时异常 | **影响**: 21 处

### 问题描述

代码中使用了 `CreatedTime` 属性名，但 `BaseFullEntity` 基类中定义的是 `CreationTime`。

### BaseFullEntity 源码

```csharp
public abstract class BaseFullEntity : IBaseFullEntity
{
    public long Id { get; set; } = YitIdHelper.NextId();
    public long Creator { get; set; }
    public DateTime CreationTime { get; set; }   // ← 属性名是 CreationTime
    public long Modifier { get; set; }
    public DateTime ModificationTime { get; set; }
    public bool IsDeleted { get; set; }
}
```

### 错误写法

```csharp
// ❌ CreatedTime 不存在于 BaseFullEntity
var list = await expr.OrderBy(f => f.CreatedTime, OrderByType.Desc)

// ❌ DTO 中也使用了错误的属性名
public class FactoryDto { public DateTime CreatedTime { get; set; } }
```

### 正确写法

```csharp
// ✅ 使用 CreationTime
var list = await expr.OrderByDescending(x => x.CreationTime)

// ✅ DTO 中使用正确属性名
public class FactoryDto { public DateTime CreationTime { get; set; } }
```

### 受影响文件 (21 处)

- **DTO (4 个)**: FactoryDto, LineDto, ProcessDto, WorkshopDto
- **Service (17 处)**: FactoryService(1), LineService(1), WorkshopService(1), EquipmentService(2), DefectRecordService(10), QCOrderService(3), WorkOrderService(3)

### 预防措施

- ✅ SKILL.md §5.3 明确列出了所有 BaseFullEntity 属性
- ✅ 审查清单包含 `CreationTime` 拼写检查

---

## 第八轮：BaseData.Tests 测试文件

**发现时间**: 2026-05-13 23:15 | **严重程度**: 🔴 阻塞编译 | **影响**: 6 个文件

### 问题汇总

| 问题类型 | 描述 | 影响 |
|---------|------|------|
| 中文乱码 | 源文件编码问题导致中文注释显示为乱码 | 6 个文件 |
| 错误 using | 使用了 `using Lzq.BaseData.Domain.Entities.BaseData;` 等不存在命名空间 | 6 个文件 |
| 缺少 using | Service 和 Repository 接口的 using 缺失 | 6 个文件 |
| 字符串不完整 | 多处字符串缺少闭合引号 | 4 个文件 |

### 关键修复：正确的实体命名空间

```csharp
// ❌ 错误
using Lzq.BaseData.Domain.Entities.BaseData;  // 不存在

// ✅ 正确
using Lzq.BaseData.Domain.Entities.Factory;
using Lzq.BaseData.Domain.Entities.Workshop;
using Lzq.BaseData.Domain.Entities.Line;
using Lzq.BaseData.Domain.Entities.Process;
```

### 受影响文件

- Helpers/TestDbContext.cs
- Helpers/ServiceTestBase.cs
- Services/FactoryServiceTests.cs
- Services/LineServiceTests.cs
- Services/ProcessServiceTests.cs
- Services/WorkshopServiceTests.cs

---

## 第九轮：ApiResult 扩展方法与 Seed 重载

**发现时间**: 2026-05-13 23:22 | **严重程度**: 🔴 阻塞编译 | **影响**: 2 个框架文件

### 问题 9.1：ApiResult 缺少 GetData 扩展方法

**错误信息**: `"ApiResult<List<FactoryTreeDto>>"未包含"GetData"的定义`

**根因**: 测试代码需要通过 `result.GetData<T>()` 提取泛型数据，但 `ApiResult` 没有此方法。

**修复**: 在 `Lzq.Core/Models/ApiResult.cs` 中添加扩展方法：

```csharp
public static class ApiResultExtensions
{
    public static T? GetData<T>(this ApiResult result)
    {
        if (result is ApiResult<T> typedResult)
            return typedResult.Data;
        return default;
    }

    public static T GetDataOrThrow<T>(this ApiResult result)
    {
        if (result is ApiResult<T> typedResult)
            return typedResult.Data ?? throw new InvalidOperationException("Data is null");
        throw new InvalidOperationException($"Cannot cast ApiResult to ApiResult<{typeof(T).Name}>");
    }
}
```

### 问题 9.2：Seed 方法类型推断失败

**错误信息**: `无法从用法中推断出方法"TestDbContext.Seed<T>(params T[])"的类型参数`

**根因**: 测试代码传递了多种类型的实体参数，但泛型方法 `Seed<T>(params T[])` 无法推断混合类型。

**修复**: 在 `TestDbContext.cs` 中添加 `params object[]` 重载：

```csharp
public void Seed(params object[] entities)
{
    if (entities.Length > 0)
    {
        foreach (var entity in entities)
            _client.Insertable(entity).ExecuteCommand();
    }
}
```

### 版本变更

- **Lzq.Extensions**: `0.1.35` → `0.1.36`

---

## 第十轮：AI 模块 Vue 文件中文乱码（UTF-8 编码损坏）

**发现时间**: 2026-05-15 | **严重程度**: 🟡 显示异常（不阻塞编译） | **影响**: 16 个文件

### 问题描述

AI 模块（`src/modules/ai/`）下的 Vue 文件中，所有中文文本显示为乱码。文件本身是合法 UTF-8 编码，但中文内容已被中间编码步骤损坏——正确的 UTF-8 中文字节被当作其他编码读取，又按 UTF-8 重新编码，导致产生大量"看起来像中文但实际是乱码"的 CJK 字符。

### 典型乱码对照

| 正确内容 | 损坏后显示 | 出现位置 |
|---------|-----------|---------|
| `导入` | `瀵煎叆` | import 注释 |
| `扩展消息类型` | `鎵╁睍娑堟伅绫诲瀷` | script 注释 |
| `暗黑模式` | `鏆楅粦妯″紡` | CSS 注释 |
| `新建对话` | `鏂板缓瀵硅瘽` | 按钮文本 |
| `发送` | `鍙戦€?| 按钮文本 |
| `设置智能体` | `璁剧疆鏅鸿兘浣?| 模板文本 |
| `侧边栏` | `渚ц竟鏍?| 模板文本 |

### 根因分析

- **中间编码损坏**：AI Agent 在生成或写入 Vue 文件时，文件内容的 UTF-8 字节流经过了一个"误读+重编码"的过程。原始正确的中文 UTF-8 字节被当作某种单字节编码（如 Latin-1 或 cp1252）解释为字符，然后又将这些字符按 UTF-8 重新编码写入。
- **不可逆向恢复**：虽然理论上可以通过反推字节来恢复，但实际尝试全部失败——Python `latin-1`/`cp1252` 字节回环方法因文件中混有 Latin-1 范围外的合法 Unicode 字符而失败；Python 乱码字符串匹配方法因字符在传输过程中的二次重编码而不可靠。
- **最终修复方法**：直接使用 Write 工具以完全正确的中文内容整体覆盖文件。

### 失败修复尝试

| 方法 | 命令/代码 | 失败原因 |
|------|----------|---------|
| Python latin-1 roundtrip | `content.encode('latin-1').decode('utf-8')` | `'latin-1' codec can't encode characters in position 49-51` — 文件包含 Latin-1 范围外的合法 Unicode |
| Python cp1252 roundtrip | `content.encode('cp1252').decode('utf-8')` | 同 latin-1，cp1252 也是单字节编码，遇到相同问题 |
| Python 乱码字符串匹配 | 直接对乱码字符串做 `encode('utf-8').decode('cp1252')` | 字符在 Bash 传输过程中被再次重编码，匹配失败 |

### 成功修复方法

直接使用 Write 工具重写文件内容——逐文件识别乱码位置，用正确中文完整替换：

```bash
# 1. 用 Read 工具读取完整文件内容
# 2. 逐行识别所有乱码（参考上文对照表）
# 3. 用 Write 工具以正确中文覆盖整个文件
```

### 受影响文件 (16 个)

| 子目录 | 文件 | 类型 |
|--------|------|------|
| aiAgentManage | `index.vue` | 智能体管理页面 |
| aiAgentSkill | `index.vue` | 智能体技能页面 |
| aiModelConfig | `index.vue` | 模型配置页面 |
| aiModelConfig | `add.vue` | 模型配置新增 |
| aiModelConfig | `detail.vue` | 模型配置详情 |
| apiKey | `index.vue` | API Key 管理 |
| apiKey | `add.vue` | API Key 新增 |
| apiKey | `detail.vue` | API Key 详情 |
| aiChats | `index.vue` | AI 对话主页（含 VoiceInputButton、ExtendedMessage） |
| aiChats | `index2.vue` | AI 对话简化版（无语音输入） |
| aiChats | `data.ts` | 对话类型定义 |
| aiChats | `modules/form.vue` | 对话表单弹窗 |
| aiChats | `modules/detail.vue` | 对话详情弹窗 |
| aiChats | `modules/chat.vue` | 对话消息组件 |
| aiChats | `modules/message.vue` | 单条消息组件 |
| aiChats | `locales/zh-CN.json` | 国际化文件 |

### 预防措施

- ✅ AI Agent 生成 Vue 文件后，**必须验证中文注释和文本是否正确**（不能仅靠"文件是合法 UTF-8"来判断）
- ✅ 代码审查清单增加"中文内容正确性检查"（对照上述典型乱码模式快速扫描）
- ✅ 如果发现乱码，不要尝试编码转换修复——直接用 Write 工具覆盖正确内容

---

## Lzq.MES 编译问题

**发现时间**: 2026-05-14 00:14 | **来源**: `Lzq.MES-build-issues-20260514.md`

### 问题 M1：不存在的项目引用 🔴

`Lzq.MES.WebApi.csproj` 引用了不存在的 `Modules\MES\Lzq.MES.Application` 项目。需创建模块或修正引用路径。

### 问题 M2：result.Success 应为 IsSuccess ✅

**已修复 4 个文件**: 测试代码使用了 `result.Success`（这是一个静态方法），正确用法是 `result.IsSuccess`（属性）。

### 问题 M3：csproj 缺少 PropertyGroup 🔴

7 个模块项目的 `.csproj` 缺少 `TargetFramework`、`ImplicitUsings`、`Nullable` 等必需属性。

### 问题 M4：缺少 SqlSugar.Repository using ✅

### 问题 M5：WorkReportRepository 未实现接口成员 ✅

接口定义了 `GetByWorkOrderIdAsync` 和 `GetTotalQtyByWorkOrderIdAsync`，但实现类为空。

### 问题 M6：Validator 使用错误命名空间 🔴

6 个 Validator 文件使用了 `using Lzq.Core.FluentValidation;`，应为 `using FluentValidation;`。

### 问题 M7：PageQuery 缺少 Lzq.Core.Models using ✅

6 个 Query 文件缺少 `using Lzq.Core.Models;` 导致 `PagedRequest` 无法解析。

---

## WorkOrder 模块耦合问题

**发现时间**: 2026-05-14 | **来源**: `WorkOrder-module-coupling-issues-20260514.md`

### 问题 W1：跨模块直接引用 Domain 层 🔴

`WorkOrderService` 直接注入 `ILineRepository` 和 `IProcessRepository`，违反模块解耦原则。

**修复方案**: 创建 `IReferenceDataService` 接口（在 Contracts 层）→ `ReferenceDataService` 实现（在 Application 层）→ Service 通过接口调用。

### 问题 W2：ThenBy API 错误使用 🔴

`ISugarQueryable` 的 `ThenBy` 方法不接受 `OrderByType` 参数。正确用法是多层 `OrderByDescending` 链式调用。

### 问题 W3：Service 必须实现接口 🚨

`WorkOrderService` 必须显式实现 `IWorkOrderService` 接口，方法返回类型必须完全匹配。

---

## Equipment 模块架构问题

**发现时间**: 2026-05-14 | **来源**: `Equipment-Domain-architecture-issues-20260514.md`, `Equipment-Entity-issues-20260514.md`, `architecture-decision-repository-location-20260514.md`

### 问题 E1：Domain 层引用其他 Domain 层 🔴

`Lzq.Equipment.Domain.csproj` 错误引用了 `Lzq.BaseData.Domain`。

### 问题 E2：Repository 位置决策 ✅

经架构决策确认：Repository 实现**保留在 Domain 层**（非标准 DDD，但符合项目实际）。

### 问题 E3：InspectionItemEntity 基类错误 🔴

使用了不存在的 `BaseEntity` 基类 → 改为 `BaseFullEntity`。

### 问题 E4：多实体混在同一文件 🟡

`InspectionEntity.cs` 包含三个实体类 → 拆分为独立文件。

---

## 版本发布流程

当 Lzq.Extensions 需要修改时，遵循以下标准流程：

```
1. 修改 Lzq.Extensions 源码
   ↓
2. 更新 Directory.Build.props 版本号
   ↓
3. 执行 build.bat 打包
   ↓
4. 更新 WorkBuddy 项目 Directory.Build.props 中的 LzqExtensionsVersion
   ↓
5. 构建 WorkBuddy 验证
   ↓
6. 记录修改到 problems/ 目录
```

---

## 根因分析与系统性改进建议

### 根因分类

| 类别 | 占比 | 典型问题 |
|------|------|---------|
| **Skill 文档错误** | ~40% | 实体命名空间、Repository 命名空间、ServiceBase 位置 |
| **项目模板不完整** | ~25% | csproj PropertyGroup 缺失、缺失 using |
| **API 理解偏差** | ~20% | ThenBy 用法、CreationTime 拼写、FromBody 位置 |
| **架构边界不清** | ~15% | 跨模块引用、Domain 层结构 |

### 系统性改进建议

1. **Skill 文档规范化**: 所有 skill 中的代码示例使用统一检查清单验证
2. **项目模板自动化**: 提供 dotnet template 或脚手架脚本确保新模块结构正确
3. **编译前检查脚本**: 添加 pre-build 脚本验证常见命名空间错误
4. **架构 ADR 记录**: 关键架构决策必须记录到 `problems/` 目录
5. **AI Agent 审查清单**: 每次代码生成后自动执行 10 项审查（见 SKILL.md §14.3）

---

## 累计修复统计

| 轮次 | 类别 | 数量 |
|------|------|------|
| 第一轮 | 实体 Entity 命名空间 | 13 |
| 第二轮 | Repository 实现类 | 24 |
| 第三轮 | PageQuery/Service 文件 | 21 |
| 第四轮 | IServices 接口 [FromBody] | 52 处 |
| 第五轮 | ServiceBase 命名空间 | 12 |
| 第六轮 | [FromBody] Mvc 命名空间 | 12 |
| 第七轮 | CreationTime 属性名 | 21 处 |
| 第八轮 | BaseData.Tests 测试文件 | 6 |
| 第九轮 | ApiResult 扩展 & Seed 重载 | 2 |
| 第十轮 | AI 模块 Vue 中文乱码 | 16 |
| MES 编译问题 | 多类 | 7 类 |
| WorkOrder 耦合 | 跨模块架构 | 3 类 |
| Equipment 架构 | Domain 结构 | 4 类 |
| **总计** | | **168+ 个文件/位置** |

---

> 📖 返回 [SKILL.md](./SKILL.md) | 📂 架构评估见 [problems01/](./problems01/)
