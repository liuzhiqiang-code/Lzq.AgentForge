# 模块开发指南

---

## 创建新模块的完整步骤

### 步骤 1：创建三个项目

```
Lzq.YourModule.Domain/
Lzq.YourModule.Application/
Lzq.YourModule.Application.Contracts/
```

### 步骤 2：Domain 层 - 定义实体

```csharp
// Entities/YourEntity/YourEntity.cs
using Lzq.Extensions.SqlSugar.Entities;  // ✅ 正确：BaseFullEntity 在此
using SqlSugar;

namespace Lzq.YourModule.Domain.Entities.YourEntity;

[Tenant("AgentForge")]
[SugarTable("your_table")]
public class YourEntity : BaseFullEntity  // ✅ 使用 BaseFullEntity
{
    [SugarColumn(ColumnName = "name", Length = 100)]
    public string Name { get; set; } = string.Empty;  // ✅ 必填字段初始化
    
    [SugarColumn(ColumnName = "description", Length = 500, IsNullable = true)]
    public string? Description { get; set; }  // ✅ 可选字段
    
    [SugarColumn(ColumnName = "status")]
    public EnableStatusEnum Status { get; set; } = EnableStatusEnum.Enabled;
}
```

⚠️ **重要**：
- 每个实体类放在**独立的 .cs 文件**中
- 基类使用 `BaseFullEntity`（不是 `BaseEntity`）
- 审计字段：`Id`, `Creator`, `CreationTime`, `Modifier`, `ModificationTime`, `IsDeleted`

### 步骤 3：Domain 层 - 创建仓储接口

```csharp
// IRepositories/YourEntity/IYourEntityRepository.cs
using Lzq.Extensions.SqlSugar.Repository;  // ✅ ISqlSugarRepository<T>
using Microsoft.Extensions.DependencyInjection;  // ✅ ITransientDependency

namespace Lzq.YourModule.Domain.IRepositories.YourEntity;

public interface IYourEntityRepository : ISqlSugarRepository<YourEntity>, ITransientDependency
{
    // 自定义查询方法
    Task<YourEntity?> GetByNameAsync(string name);
}
```

### 步骤 4：Domain 层 - 实现仓储

```csharp
// Repositories/YourEntity/YourEntityRepository.cs
using Lzq.Extensions.SqlSugar.Repository;  // ✅ SqlSugarRepository<T>
using Lzq.YourModule.Domain.Entities.YourEntity;
using Lzq.YourModule.Domain.IRepositories.YourEntity;

namespace Lzq.YourModule.Domain.Repositories.YourEntity;

public class YourEntityRepository : SqlSugarRepository<YourEntity>, IYourEntityRepository
{
    public async Task<YourEntity?> GetByNameAsync(string name)
    {
        return await AsQueryable().Where(x => x.Name == name).FirstAsync();
    }
}
```

### 步骤 5：Contracts 层 - 定义 DTO/Command/Query

```csharp
// Commands/YourEntityCreateCommand.cs
public record YourEntityCreateCommand(string Name, EnableStatusEnum Status);

// Queries/YourEntityPageQuery.cs
public record YourEntityPageQuery : PagedRequest
{
    public string? Keyword { get; set; }
}

// Dto/YourEntityViewDto.cs
public class YourEntityViewDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public EnableStatusEnum Status { get; set; }
}
```

### 步骤 6：Contracts 层 - 创建验证器

```csharp
using FluentValidation;  // ✅ 标准 FluentValidation

public class YourEntityCreateCommandValidator : AbstractValidator<YourEntityCreateCommand>
{
    public YourEntityCreateCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("名称不能为空");
    }
}
```

### 步骤 7：Contracts 层 - 定义服务接口

```csharp
// IServices/IYourEntityService.cs
public interface IYourEntityService
{
    Task<ApiResult<PagedResponse<YourEntityViewDto>>> PageAsync(YourEntityPageQuery query);
    Task<ApiResult<YourEntityViewDto>> GetAsync(long id);
    Task<ApiResult<long>> CreateAsync(YourEntityCreateCommand cmd);
    Task<ApiResult<bool>> UpdateAsync(YourEntityUpdateCommand cmd);
    Task<ApiResult<bool>> DeleteAsync(long id);
}
```

> ⚠️ **重要**：接口中不要添加 `[FromBody]` 等特性，除非明确需要支持远程调用。

### 步骤 8：Application 层 - 实现服务

```csharp
using Microsoft.AspNetCore.Builder;  // ServiceBase
using Microsoft.AspNetCore.Mvc;      // [FromBody]
using Lzq.Core.Models;              // PagedRequest/PagedResponse
using Lzq.Extensions.SqlSugar.Repository;

public class YourEntityService : ServiceBase, IYourEntityService
{
    public YourEntityService() : base("/api/v1/yourmodule/entity") { }
    
    private ISqlSugarRepository<YourEntity> Repo => GetRequiredService<ISqlSugarRepository<YourEntity>>();

    [OpenApiTag("yourmodule/entity")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<YourEntityViewDto>>> PageAsync([FromBody] YourEntityPageQuery query)
    {
        var expr = Repo.AsQueryable();
        if (!string.IsNullOrEmpty(query.Keyword))
            expr = expr.Where(x => x.Name.Contains(query.Keyword));
        
        RefAsync<int> total = 0;
        var items = await expr.OrderByDescending(x => x.CreationTime)
                             .ToPageListAsync(query.Page, query.PageSize, total);
        
        return ApiResult.Success(new PagedResponse<YourEntityViewDto>(
            items.Map<List<YourEntityViewDto>>(), total));
    }

    [OpenApiTag("yourmodule/entity")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] YourEntityCreateCommand cmd)
    {
        var entity = cmd.Map<YourEntity>();
        await Repo.InsertAsync(entity);
        return ApiResult.Success(entity.Id);
    }

    [OpenApiTag("yourmodule/entity")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        await Repo.DeleteAsync(x => x.Id == id);
        return ApiResult.Success(true);
    }
}
```

---

## 跨模块依赖规范

### 原则
- Application 层只能引用本模块的 Domain 层和 Contracts 层
- 引用其他模块数据必须通过目标模块的 Contracts 层接口

### 模式一：使用目标模块的 Application.Contracts 服务

```csharp
// 直接注入其他模块的 Service 接口
public class YourService : ServiceBase, IYourService
{
    private ILineService LineService => GetRequiredService<ILineService>();
    
    public async Task DoSomethingAsync()
    {
        var result = await LineService.GetByIdAsync(lineId);
    }
}
```

### 模式二：创建引用服务（推荐用于复杂查询）

当需要批量查询或自定义数据访问时，在本模块的 Application 层创建引用服务：

1. 在 Contracts 层定义接口（`IReferenceDataService`）
2. 在 Application 层实现（`ReferenceDataService`）
3. Application 层 .csproj 中添加对目标 Domain 层的引用

---

## 种子数据

```csharp
using Lzq.Extensions.SqlSugar.Entities;  // ✅ BaseSeedData<T>

public class YourEntitySeedData : BaseSeedData<YourEntity>
{
    public override List<YourEntity> GetSeedData()
    {
        return new List<YourEntity>
        {
            new YourEntity { Id = 1, Name = "默认数据" }
        };
    }
}
```
