# ApiTreeSelect - 远程数据树选择

## 用法示例

```typescript
{
  component: 'ApiTreeSelect',
  componentProps: {
    api: getAllMenusApi,           // API 函数
    labelField: 'name',           // 标签字段名
    valueField: 'path',           // 值字段名
    childrenField: 'children',    // 子级数据字段名
  },
  fieldName: 'apiTree',
  label: 'ApiTreeSelect',
}
```

## componentProps 配置

| 属性 | 说明 | 类型 | 默认值 |
|------|------|------|---------|
| `api` | 获取数据的 API 函数 | `() => Promise<OptionsItem[]>` | - |
| `labelField` | 标签字段名 | `string` | `label` |
| `valueField` | 值字段名 | `string` | `value` |
| `childrenField` | 子级字段名 | `string` | `children` |

## 底层原理

`ApiSelect` 和 `ApiTreeSelect` 都基于 `ApiComponent` 包装。如果需要为其他组件添加远程数据能力，可直接使用 `ApiComponent`：

```vue
<script lang="ts" setup>
import { ApiComponent } from '@vben/common-ui';
import { Cascader } from 'ant-design-vue';

function fetchApi(): Promise<Record<string, any>> {
  return new Promise((resolve) => {
    setTimeout(() => resolve(treeData), 1000);
  });
}
</script>

<template>
  <ApiComponent
    :api="fetchApi"
    :component="Cascader"
    :immediate="false"
    children-field="children"
    loading-slot="suffixIcon"
    visible-event="onDropdownVisibleChange"
  />
</template>
```
