# ApiSelect - 远程数据下拉框

> 核心特性：自动从 API 获取选项数据，支持搜索、自动选择等高级功能。

## 方式一：简化用法（推荐 ⭐）

在 `api` 函数内完成数据获取和转换：

```typescript
{
  component: 'ApiSelect',
  fieldName: 'lineId',
  label: $t('workorder.workorder.lineId'),
  componentProps: {
    api: async () => {
      const res = await getLinePage({ page: 1, pageSize: 1000 });
      return (res.list || []).map((item: any) => ({
        label: `${item.code} - ${item.name}`,
        value: item.id,
      }));
    },
    showSearch: true,
    allowClear: true,
    placeholder: $t('workorder.workorder.linePlaceholder'),
  },
}
```

**优势**：代码简洁、数据转换与 API 调用在一起、自动处理 loading

## 方式二：标准用法（使用 afterFetch）

```typescript
{
  component: 'ApiSelect',
  componentProps: {
    api: getSomePage,
    afterFetch: (data: any[]) => data.map((item: any) => ({
      label: item.name,
      value: item.id,
    })),
    autoSelect: 'first',
    placeholder: '请选择',
  },
  fieldName: 'someId',
  label: '远程下拉框',
}
```

适用场景：API 函数在多个地方复用时，分离数据转换逻辑

## 两种方式对比

| 维度 | 方式一（简化） | 方式二（标准） |
|------|----------------|---------------|
| 代码简洁度 | ✅ 更简洁 | 需要 afterFetch |
| 数据转换位置 | 在 api 函数内 | 在 afterFetch 回调 |
| 复用性 | API 函数专用于此组件 | API 函数可共用 |
| 适用场景 | 简单场景、快速开发 | 复杂场景、需要复用 |

## componentProps 完整配置

| 属性 | 说明 | 类型 | 默认值 |
|------|------|------|---------|
| `api` | 获取数据的 API 函数 | `() => Promise<OptionsItem[]>` | - |
| `params` | 传递给 api 的参数（响应式） | `Record<string, any>` | - |
| `afterFetch` | API 返回后的数据转换回调 | `(data) => OptionsItem[]` | - |
| `autoSelect` | 自动选择选项 | `'first' \| 'last' \| 'one' \| false` | `false` |
| `filterOption` | 是否允许本地过滤 | `boolean \| function` | `true` |
| `showSearch` | 是否显示搜索框 | `boolean` | `false` |
| `immediate` | 是否立即加载数据 | `boolean` | `true` |

## 高级用法：远程搜索（带防抖）

```typescript
import { useDebounceFn } from '@vueuse/core';

const keyword = ref('');
const fetching = ref(false);

{
  component: 'ApiSelect',
  componentProps: () => ({
    api: fetchRemoteOptions,
    filterOption: false,              // 禁止本地过滤（使用远程搜索）
    onSearch: useDebounceFn((value: string) => {
      keyword.value = value;
    }, 300),
    params: { keyword: keyword.value || undefined },
    showSearch: true,
    notFoundContent: fetching.value ? undefined : null,
  }),
  fieldName: 'remoteSearch',
  label: '远程搜索',
  renderComponentContent: () => ({
    notFoundContent: fetching.value ? h(Spin) : undefined,
  }),
  rules: 'selectRequired',
}
```

## 反模式：Select + options:[] + onMounted（禁止使用）

**❌ 错误模式**：用 `Select` 组件 + 空 `options:[]` + `onMounted` 异步加载数据，然后直接修改 schema 对象。

```typescript
// data.ts - ❌ 错误
{
  component: 'Select',
  componentProps: {
    options: [],  // 空数组，等待 form.vue 注入
    fieldNames: { label: 'name', value: 'id' },
  },
  fieldName: 'workshopId',
}
```

```vue
// form.vue - ❌ 错误
<script lang="ts" setup>
const schema = useSchema();           // ❌ 保存 schema 引用
const options = ref([]);              // ❌ 手动管理选项状态

function updateSchema() {
  schema[0].componentProps!.options = options.value; // ❌ 绕过表单状态管理
}
async function loadOptions() {
  const res = await getWorkshopPage({ page: 1, pageSize: 999 });
  options.value = res?.items || [];
  updateSchema();
}
onMounted(() => { loadOptions(); });  // ❌ 首次挂载才加载，复用可能拿到过期数据
</script>
```

**问题分析**：
- `options: []` 在数据加载前为空，快速打开弹窗时下拉框无数据
- 直接修改 `schema[0].componentProps!.options` 绕过 VbenForm 内部状态管理
- `onMounted` 只在首次挂载执行，组件复用时可能拿到过期数据
- 缺少 `ref` 导入时导致运行时崩溃

**✅ 正确模式**：在 data.ts 中用 `ApiSelect` + 内联 `api` 函数完成一切：

```typescript
// data.ts - ✅ 正确
import { getWorkshopPage } from '#/modules/basadata/api/workshop';

{
  component: 'ApiSelect',
  componentProps: {
    api: async () => {
      const res = await getWorkshopPage({ page: 1, pageSize: 999 });
      return (res?.items || []).map((item) => ({ label: item.name, value: item.id }));
    },
    showSearch: true,
    allowClear: true,
    placeholder: '请选择车间',
  },
  fieldName: 'workshopId',
}
```

```vue
// form.vue - ✅ 正确：无 onMounted、无 schema 变量、无手动更新
const [Form, formApi] = useVbenForm({
  schema: useSchema(),   // ✅ 直接调用，不保存引用
  showDefaultActions: false,
});
```

## 修复步骤（反模式 → 正确模式）

| 步骤 | data.ts | form.vue |
|------|---------|----------|
| 1 | `Select` → `ApiSelect` | 移除 `onMounted` 及相关函数 |
| 2 | 移除 `options: []` 和 `fieldNames` | 移除 `xxxOptions` ref 变量 |
| 3 | 添加 `api: async () => {...}` 并引入 API | 移除 `loadXxxOptions()` 函数 |
| 4 | - | 移除 `updateSchema()` 函数 |
| 5 | - | `schema: useSchema()` 直接传参（不用变量） |
| 6 | - | 移除不再需要的 API 导入 |

## 已修复记录（2026-05-15）

| 模块 | 修复内容 |
|------|---------|
| `basadata/line` | 车间下拉框 → ApiSelect + `getWorkshopPage` |
| `basadata/process` | 产线下拉框 → ApiSelect + `getLinePage` |
| `basadata/workshop` | 工厂下拉框 → ApiSelect + `getFactoryPage` |
| `equipment/inspection/plan` | 缺少 `ref` 导入 → 已修复 |
| `equipment/maintenance/plan` | 缺少 `ref` 导入 → 已修复 |
