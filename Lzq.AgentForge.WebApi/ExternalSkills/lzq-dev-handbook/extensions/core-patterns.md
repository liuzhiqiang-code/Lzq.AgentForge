# 09 - Lzq.Core 核心模式参考

> 来源: lzq-core | 状态: ✅ 已验证

## 完整 API 端点模式

```csharp
public class XxxService : ServiceBase, IXxxService
{
    public XxxService() : base("/api/v1/module/entity") { }

    // 分页查询
    [OpenApiTag("module/entity"), RoutePattern("page", true)]
    public async Task<ApiResult<PagedResponse<XxxViewDto>>> PageAsync(XxxPageQuery query)
    {
        RefAsync<int> total = 0;
        var items = await Repo.AsQueryable()
            .OrderByDescending(x => x.CreationTime)
            .ToPageListAsync(query.Page, query.PageSize, total);
        return ApiResult.Success(new PagedResponse<XxxViewDto>(
            items.Map<List<XxxViewDto>>(), total));
    }

    // 详情查询
    [OpenApiTag("module/entity"), RoutePattern("{id}", false)]
    public async Task<ApiResult<XxxViewDto>> GetAsync(long id)
    {
        var entity = await Repo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("记录不存在");
        return ApiResult.Success(entity.Map<XxxViewDto>());
    }

    // 创建
    [OpenApiTag("module/entity"), RoutePattern("create", true)]
    public async Task<ApiResult<long>> CreateAsync(XxxCreateCommand cmd)
    {
        var entity = cmd.Map<XxxEntity>();
        await Repo.InsertAsync(entity);
        return ApiResult.Success(entity.Id);
    }

    // 更新
    [OpenApiTag("module/entity"), RoutePattern("update", true)]
    public async Task<ApiResult<bool>> UpdateAsync(XxxUpdateCommand cmd)
    {
        var entity = await Repo.GetByIdAsync(cmd.Id)
            ?? throw new UserFriendlyException("记录不存在");
        cmd.Map(entity);
        await Repo.UpdateAsync(entity);
        return ApiResult.Success(true);
    }

    // 删除
    [OpenApiTag("module/entity"), RoutePattern("delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        await Repo.DeleteAsync(x => x.Id == id);
        return ApiResult.Success(true);
    }

    // 依赖声明
    private ISqlSugarRepository<XxxEntity> Repo => 
        GetRequiredService<ISqlSugarRepository<XxxEntity>>();
}
```

## RoutePattern 使用说明

| 参数 | 说明 |
|---|---|
| `pattern` | 相对路由模板 |
| `isPost` | `true`=POST, `false`=GET |

路由组成：`{basePath}/{pattern}`

## 树形数据构建模式

```csharp
private List<XxxViewDto> BuildTree(List<XxxViewDto> all, long? parentId)
{
    return all.Where(d => d.Pid == parentId)
        .Select(d => new XxxViewDto
        {
            Id = d.Id, Name = d.Name,
            Children = BuildTree(all, d.Id)
        }).ToList();
}
```
