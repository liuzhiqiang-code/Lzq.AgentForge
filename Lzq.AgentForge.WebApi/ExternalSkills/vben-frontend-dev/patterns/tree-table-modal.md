# 模式 1：树形表格 + 弹窗表单

> 适用场景：部门管理、分类管理等树形数据结构
> 参考：`playground/src/views/system/dept/list.vue`

## Page 核心结构

```typescript
// 1. 导入
import { Page, useVbenModal } from '@vben/common-ui';
import { Plus } from '@vben/icons';
import { Button, message } from 'ant-design-vue';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { $t } from '#/locales';

// 2. Modal 关联弹窗表单
const [FormModal, formModalApi] = useVbenModal({
  connectedComponent: Form,
  destroyOnClose: true,
});

// 3. Grid 配置（树形 + toolbar）
const [Grid, gridApi] = useVbenVxeGrid({
  gridOptions: {
    columns: useColumns(onActionClick),
    treeConfig: { parentField: 'pid', rowField: 'id' },
    proxyConfig: {
      ajax: { query: async () => await getList() },
    },
    toolbarConfig: { custom: true, refresh: true, zoom: true },
  } as VxeTableGridOptions,
});
```

## Form 弹窗核心结构

```typescript
import { useVbenModal } from '@vben/common-ui';
import { useVbenForm } from '#/adapter/form';
import { useSchema } from '../data';

const [Form, formApi] = useVbenForm({
  schema: useSchema(),
  showDefaultActions: false,
});

const [Modal, modalApi] = useVbenModal({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (valid) {
      modalApi.lock();
      const data = await formApi.getValues();
      await (id.value ? updateApi(id.value, data) : createApi(data));
      modalApi.close();
      emit('success');
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = modalApi.getData<XxxApi.XxxItem>();
      formApi.setValues(data || {});
    }
  },
});
```

## 关键点
- `treeConfig` 指定 `parentField` 和 `rowField` 实现树形结构
- `proxyConfig.ajax.query` 直接返回完整树数据（不分页）
- 弹窗用 `useVbenModal` 的 `connectedComponent` 关联表单组件
- `onOpenChange` 中通过 `modalApi.getData()` 获取编辑数据
