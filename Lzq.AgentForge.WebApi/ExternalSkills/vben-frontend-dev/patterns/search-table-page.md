# 模式 2：搜索 + 表格 + 分页

> 适用场景：带搜索条件的分页列表（角色、用户、工单等）
> 参考：`playground/src/views/system/role/list.vue`

## 核心结构

```typescript
const [Grid, gridApi] = useVbenVxeGrid({
  formOptions: {
    schema: useGridFormSchema(),      // 搜索表单 schema
    submitOnChange: true,
  },
  gridOptions: {
    columns: useColumns(onActionClick),
    proxyConfig: {
      ajax: {
        query: async ({ page }, formValues) => {
          return await getList({
            page: page.currentPage,
            pageSize: page.pageSize,
            ...formValues,
          });
        },
      },
    },
    toolbarConfig: { custom: true, refresh: true, search: true, zoom: true },
    pagerConfig: {},                   // 开启分页
  } as VxeTableGridOptions,
});
```

## 关键点
- `formOptions.schema` 定义顶部的搜索表单字段
- `proxyConfig.ajax.query` 回调参数 `{ page, sort, filters, formValues }`
- `pagerConfig: {}` 启用分页器
- `submitOnChange: true` 使筛选条件变化时自动提交
- `fieldMappingTime` 可将日期范围组件映射为两个独立字段：
  ```typescript
  formOptions: {
    fieldMappingTime: [['createTime', ['startTime', 'endTime']]],
  }
  ```
