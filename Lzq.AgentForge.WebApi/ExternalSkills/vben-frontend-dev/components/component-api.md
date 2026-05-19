# Vben 组件 API 速查

## useVbenModal

```typescript
const [Modal, modalApi] = useVbenModal({
  connectedComponent?: Component,   // 关联的弹窗组件
  destroyOnClose?: boolean,         // 关闭时销毁
  onConfirm?: () => Promise<void>,  // 确认回调
  onOpenChange?: (isOpen: boolean) => void, // 打开/关闭回调
});

// modalApi 方法
modalApi.open();                      // 打开
modalApi.close();                     // 关闭
modalApi.setData(data);               // 设置传递数据
modalApi.getData<T>();                // 获取传递数据
modalApi.lock(loading?: boolean);     // 锁定/解锁
modalApi.lock(false);                 // 解锁
```

## useVbenDrawer

API 与 `useVbenModal` 完全一致：

```typescript
const [Drawer, drawerApi] = useVbenDrawer({
  connectedComponent?: Component,
  destroyOnClose?: boolean,
  onConfirm?: () => Promise<void>,
  onOpenChange?: (isOpen: boolean) => void,
});

// drawerApi 方法（同 modalApi）
drawerApi.open();
drawerApi.close();
drawerApi.setData(data);
drawerApi.getData<T>();
drawerApi.lock(loading?: boolean);
```

## useVbenForm

```typescript
const [Form, formApi] = useVbenForm({
  schema: VbenFormSchema[],         // 表单字段配置
  layout?: 'horizontal' | 'vertical',
  showDefaultActions?: boolean,      // 是否显示默认操作按钮
  wrapperClass?: string,
  fieldMappingTime?: [string, [string, string], string?][], // 时间字段映射
});

// formApi 方法
formApi.validate();                   // 校验
formApi.getValues();                  // 获取值
formApi.setValues(data);              // 设置值
formApi.resetForm();                  // 重置
formApi.updateSchema(partialSchema);  // 动态更新字段配置
```

## VbenFormSchema 结构

```typescript
interface VbenFormSchema {
  component: string;                  // 组件名（见下方白名单）
  fieldName: string;                  // 字段名
  label: string;                      // 标签
  defaultValue?: any;                 // 默认值
  rules?: string | ZodSchema;        // 'required' | z.string().min(2) | ...
  componentProps?: Record<string, any> | (() => Record<string, any>); // 组件 props（支持函数）
  dependencies?: {                    // 字段依赖
    componentProps?: (values: any) => Record<string, any>;  // 动态 props
    trigger?: (values: any, form: any) => void;             // 变化回调
    triggerFields?: string[];                               // 监听字段
  };
  formItemClass?: string;             // 表单项类名
  modelPropName?: string;             // v-model 属性名
  renderComponentContent?: () => Record<string, any>; // 动态渲染插槽
}
```

## useVbenVxeGrid

```typescript
const [Grid, gridApi] = useVbenVxeGrid({
  formOptions?: {                     // 搜索表单配置
    schema: VbenFormSchema[],
    submitOnChange?: boolean,
    fieldMappingTime?: [string, [string, string], string?][],
  },
  gridOptions: {
    columns: VxeTableGridOptions['columns'],
    height?: 'auto' | number,
    keepSource?: boolean,
    pagerConfig?: { enabled?: boolean },
    treeConfig?: { parentField: string, rowField: string },
    proxyConfig: {
      ajax: {
        query: async ({ page, sort, filters }, formValues) => response,
      },
    },
    toolbarConfig?: { custom, export, refresh, search, zoom },
    rowConfig?: { keyField: string },
  },
});

gridApi.query();                      // 刷新（重新查询）
```

## 表单组件白名单（component 字段取值）

| 组件名 | 说明 | 来源 |
|--------|------|------|
| `Input` | 文本输入 | ant-design-vue |
| `Select` | 下拉选择（静态数据） | ant-design-vue |
| `RadioGroup` | 单选组 | ant-design-vue |
| `Checkbox` | 复选框 | ant-design-vue |
| `CheckboxGroup` | 多选组 | ant-design-vue |
| `Switch` | 开关 | ant-design-vue |
| `Textarea` | 文本域 | ant-design-vue |
| `InputNumber` | 数字输入 | ant-design-vue |
| `DatePicker` | 日期选择 | ant-design-vue |
| `RangePicker` | 日期范围 | ant-design-vue |
| `TimePicker` | 时间选择 | ant-design-vue |
| `ApiSelect` | API 远程下拉 | @vben/common-ui |
| `ApiTreeSelect` | API 远程树选择 | @vben/common-ui |
| `AutoComplete` | 自动完成 | ant-design-vue |
| `Upload` | 文件上传 | ant-design-vue |
| `Rate` | 评分 | ant-design-vue |
| `Divider` | 分割线 | ant-design-vue |
| `IconPicker` | 图标选择器 | @vben/common-ui |

## 表格列渲染器（cellRender.name 取值）

| 渲染器 | 说明 |
|--------|------|
| `CellTag` | 标签（常用于 status） |
| `CellSwitch` | 开关（带确认） |
| `CellOperation` | 操作按钮组 |
| `CellImage` | 图片 |
| `CellLink` | 链接 |

## 动态 componentProps（函数形式）

某些场景下，componentProps 需要是动态计算的：

```typescript
{
  component: 'ApiSelect',
  componentProps: () => ({
    api: someApi,
    placeholder: fetching.value ? '加载中...' : '请选择',
    notFoundContent: fetching.value ? h(Spin) : null,
  }),
  fieldName: 'dynamicProps',
  label: '动态 Props',
  renderComponentContent: () => ({
    notFoundContent: fetching.value ? h(Spin) : undefined,
  }),
}
```

**关键点**：
- `componentProps` 可以是对象或函数（返回对象）
- 函数形式可以访问响应式数据（ref、computed 等）
- `renderComponentContent` 用于动态渲染组件插槽
