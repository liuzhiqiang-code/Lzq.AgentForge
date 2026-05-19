# BaseData 单元测试总结

## 测试项目概览

测试项目位置: `Modules/BaseData/Lzq.BaseData.Tests`

### 测试辅助类

| 文件 | 说明 |
|------|------|
| `Helpers/ServiceTestBase.cs` | 所有测试类的基类，提供内存数据库、仓储和服务实例 |
| `Helpers/TestDbContext.cs` | 基于 SQLite 内存数据库的测试上下文，每次测试隔离 |

### 测试覆盖

#### FactoryServiceTests (17 个测试)
| 方法 | 测试用例 |
|------|----------|
| PageAsync | ✅ 分页返回、Code 筛选、Name 筛选、Status 筛选、分页计算 |
| TreeAsync | ✅ 返回完整树、空数据库 |
| CreateAsync | ✅ 创建工厂、重复 Code 校验、Address 字段持久化 |
| UpdateAsync | ✅ 更新工厂、工厂不存在、更新 Address |
| DeleteAsync | ✅ 软删除、ID 不存在 |
| BatchDeleteAsync | ✅ 批量软删除、空列表校验 |

#### WorkshopServiceTests (20 个测试)
| 方法 | 测试用例 |
|------|----------|
| PageAsync | ✅ 分页返回、FactoryId 筛选、Name 筛选、Status 筛选、分页计算 |
| ListByFactoryAsync | ✅ 返回启用车间、空工厂、空列表场景 |
| TreeAsync | ✅ 返回车间产线树、错误 FactoryId |
| CreateAsync | ✅ 创建车间、工厂不存在、重复 Code、Manager 字段持久化 |
| UpdateAsync | ✅ 更新车间、车间不存在、更新 Manager |
| DeleteAsync | ✅ 软删除、ID 不存在 |
| BatchDeleteAsync | ✅ 批量软删除、空列表校验 |

#### LineServiceTests (19 个测试)
| 方法 | 测试用例 |
|------|----------|
| GetByIdAsync | ✅ 返回产线、ID 不存在 |
| PageAsync | ✅ 分页返回、WorkshopId 筛选、Name 筛选、Status 筛选、分页计算 |
| ListByWorkshopAsync | ✅ 返回启用产线、空车间 |
| CreateAsync | ✅ 创建产线、车间不存在、重复 Code |
| UpdateAsync | ✅ 更新产线、产线不存在、更新 Status |
| DeleteAsync | ✅ 软删除、ID 不存在 |
| BatchDeleteAsync | ✅ 批量软删除、空列表校验 |

#### ProcessServiceTests (21 个测试)
| 方法 | 测试用例 |
|------|----------|
| PageAsync | ✅ 分页返回、LineId 筛选、Name 筛选、Status 筛选、分页计算、Sequence 排序 |
| ListByLineAsync | ✅ 返回启用工序、空产线 |
| CreateAsync | ✅ 创建工序、产线不存在、重复 Code、Sequence 和 StandardHours 持久化、零工时 |
| UpdateAsync | ✅ 更新工序、工序不存在、更新 Sequence/StandardHours/Status |
| DeleteAsync | ✅ 软删除、ID 不存在 |
| BatchDeleteAsync | ✅ 批量软删除、空列表校验 |

## 测试策略

### 测试隔离
- 每个测试类继承 `ServiceTestBase`
- 每个测试使用独立的 `TestDbContext`（内存 SQLite 数据库）
- 通过反射注入 `IServiceProvider` 解决 ServiceBase 依赖问题

### 测试模式
```csharp
public class XxxServiceTests : ServiceTestBase
{
    // Arrange - 准备测试数据
    Db.Seed(new Entity { ... });

    // Act - 执行服务方法
    var result = await XxxService.MethodAsync(...);

    // Assert - 验证结果
    result.IsSuccess.Should().BeTrue();
    // ...
}
```

### 关键修复
1. **SqlSugar Insertable<T>** - 使用 `InsertableByObject` 替代不支持的 `Insertable<object>`
2. **内存数据库连接** - `IsAutoCloseConnection=false` 防止连接关闭时数据库被销毁

## 运行测试

```bash
cd code/Modules/BaseData/Lzq.BaseData.Tests
dotnet test
```

## 注意事项

1. 需要配置 NuGet.Config 解决环境变量问题（ProgramFiles）
2. 测试依赖 Lzq.BaseData.Application 项目，需要先还原 NuGet 包
