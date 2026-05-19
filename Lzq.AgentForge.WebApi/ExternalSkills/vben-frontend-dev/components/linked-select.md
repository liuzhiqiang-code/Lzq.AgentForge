# 表单联动选择（ApiSelect dependencies）

> 使用 `ApiSelect` 组件实现表单字段联动：级联选择、动态过滤、字段清空、动态参数传递。

## 使用场景

- 级联选择：工厂 → 车间 → 产线
- 动态过滤：根据上游字段值，动态加载下游选项
- 字段联动：上游变化时，自动清空下游字段值
- 动态参数传递：将上游字段值作为参数传给下游 API

## 模式 1：上游选择变化后清空下游字段

上游字段变化时，清空下游字段（避免脏数据）：

```typescript
{
  component: 'ApiSelect',
  componentProps: {
    allowClear: true,
    showSearch: true,
    placeholder: '请选择车间',
    class: 'w-full',
    api: async () => {
      const res = await getWorkshopPage({ page: 1, pageSize: 999 });
      return (res?.items || []).map((item) => ({
        label: item.name,
        value: item.id,
      }));
    },
  },
  fieldName: 'workshopId',
  label: '车间',
  rules: 'required',
  dependencies: {
    trigger(values, form) {
      form.setFieldValue('lineId', undefined);  // 清空产线
    },
    triggerFields: ['workshopId'],
  },
  formItemClass: 'md:col-span-1 col-span-2',
}
```

**关键点**：
- `dependencies.trigger(values, form)` 接收表单实例，可调用 `form.setFieldValue` 清空下游
- `triggerFields` 指定监听哪些字段的变化

## 模式 2：下游选择根据上游动态加载

下游选项需要根据上游字段值动态过滤：

```typescript
{
  component: 'ApiSelect',
  fieldName: 'lineId',
  label: '产线',
  dependencies: {
    componentProps(values) {
      return {
        allowClear: true,
        showSearch: true,
        class: 'w-full',
        placeholder: values.workshopId ? '请选择产线' : '请先选择车间',
        params: { workshopId: values.workshopId },
        api: async ({ workshopId }) => {
          if (!workshopId) return [];
          const res = await getLinePage({ page: 1, pageSize: 999, workshopId });
          return (res?.items || []).map((item) => ({
            label: `${item.code} - ${item.name}`,
            value: item.id,
          }));
        },
      };
    },
    triggerFields: ['workshopId'],
  },
  formItemClass: 'md:col-span-1 col-span-2',
}
```

**关键点**：
- `componentProps(values)` 是函数形式，根据 `values` 动态返回配置
- `api` 函数接收 `params` 中定义的参数
- 需要判断上游字段是否有值，无值时返回空数组

## 模式 3：完整三级联动（工厂 → 车间 → 产线）

```typescript
// 1. 工厂选择
{
  component: 'ApiSelect',
  componentProps: {
    allowClear: true,
    showSearch: true,
    class: 'w-full',
    placeholder: '请选择工厂',
    api: async () => {
      const res = await getFactoryPage({ page: 1, pageSize: 999 });
      return (res?.items || []).map((item) => ({
        label: item.name,
        value: item.id,
      }));
    },
  },
  fieldName: 'factoryId',
  label: '工厂',
  rules: 'required',
  dependencies: {
    trigger(values, form) {
      form.setFieldValue('workshopId', undefined);
      form.setFieldValue('lineId', undefined);
    },
    triggerFields: ['factoryId'],
  },
  formItemClass: 'md:col-span-1 col-span-2',
},
// 2. 车间选择（依赖工厂） - 见模式 2
// 3. 产线选择（依赖车间） - 见模式 2（triggerFields 改为 ['workshopId']）
```

## 注意事项

1. **API 函数位置**：上游字段的 API 放在 `componentProps.api` 中；下游字段的 API 放在 `dependencies.componentProps(values).api` 中
2. **空值处理**：下游字段 API 函数必须判断上游值是否存在，无值时返回空数组
3. **字段清空**：使用 `form.setFieldValue(fieldName, undefined)` 清空下游字段
4. **triggerFields**：必须正确配置，否则依赖不会触发
5. **placeholder 动态更新**：可在 `componentProps(values)` 中根据上游值动态设置提示语
6. **class: 'w-full'**：所有 ApiSelect 必须加此样式

## 参考文件

- `modules/basadata/views/line/data.ts` - 车间→产线联动示例
- `modules/basadata/views/workshop/data.ts` - 工厂→车间联动示例
- `modules/basadata/views/process/data.ts` - 产线→工序联动示例
