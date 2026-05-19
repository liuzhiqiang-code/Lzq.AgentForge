# 模式 3：表格 + 抽屉表单

> 适用场景：表单字段较多、需要树形选择等复杂交互
> 参考：`playground/src/views/system/role/modules/form.vue`

## 列表页

```typescript
// 列表页
const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});
```

## 表单页

```typescript
// 表单页
const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    drawerApi.lock();
    const values = await formApi.getValues();
    await (id.value ? updateApi(id.value, values) : createApi(values));
    drawerApi.close();
    emit('success');
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData();
      formApi.setValues(data || {});
    }
  },
});
```

## 关键点
- `useVbenDrawer` 与 `useVbenModal` API 完全一致（方法名相同）
- 抽屉适合字段较多、需要更多空间的表单场景
- `destroyOnClose: true` 确保每次打开时表单重新初始化
