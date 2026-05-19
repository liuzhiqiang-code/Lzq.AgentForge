# DatePicker / RangePicker 标准配置

> 本文件记录 Vben Admin 中 `DatePicker` 组件的标准配置用法。

## 标准配置模板

所有 `DatePicker` 组件应使用以下标准配置：

```typescript
{
  component: 'DatePicker',
  componentProps: {
    class: 'w-full',                      // 占满表单项宽度
    format: 'YYYY-MM-DD HH:mm:ss',        // 显示格式
    valueFormat: 'YYYY-MM-DD HH:mm:ss',   // 输出格式（提交给后端的值）
  },
  fieldName: 'fieldName',
  label: $t('module.fieldName'),
  formItemClass: 'md:col-span-1 col-span-2',
}
```

## 关键点

1. **class: 'w-full'**：让日期选择器占满表单项的宽度，保持 UI 一致性
2. **format**：控制日期在界面上的显示格式
3. **valueFormat**：控制提交给后端的数据格式，必须与后端 API 期望的格式一致
4. **推荐使用 'YYYY-MM-DD HH:mm:ss'**：包含时分秒，适合大多数业务场景

## Ant Design Vue DatePicker 属性

| 属性 | 说明 | 默认值 |
|------|------|--------|
| `format` | 展示格式 | 'YYYY-MM-DD' |
| `valueFormat` | 绑定值的格式 | 'YYYY-MM-DD' |
| `showTime` | 是否显示时间选择 | false |

## 场景 1：仅日期（不含时间）

```typescript
{
  component: 'DatePicker',
  componentProps: {
    class: 'w-full',
    format: 'YYYY-MM-DD',
    valueFormat: 'YYYY-MM-DD',
  },
  fieldName: 'planDate',
  label: $t('module.planDate'),
}
```

## 场景 2：日期时间（含时分秒）- 推荐

```typescript
{
  component: 'DatePicker',
  componentProps: {
    class: 'w-full',
    format: 'YYYY-MM-DD HH:mm:ss',
    valueFormat: 'YYYY-MM-DD HH:mm:ss',
  },
  fieldName: 'plannedStart',
  label: $t('module.plannedStart'),
}
```

## RangePicker（日期范围）

配合 `fieldMappingTime` 使用，将日期范围组件映射为两个独立字段：

```typescript
// data.ts - schema
{
  component: 'RangePicker',
  fieldName: 'rangePicker',
  label: '日期范围',
}

// list.vue - useVbenVxeGrid 配置
formOptions: {
  fieldMappingTime: [['rangePicker', ['startTime', 'endTime'], 'YYYY-MM-DD']],
}
```

## 检查清单

添加或修改 DatePicker 时确认：
- [ ] 包含 `class: 'w-full'`
- [ ] 包含 `format` 属性
- [ ] 包含 `valueFormat` 属性
- [ ] `format` 和 `valueFormat` 保持一致
- [ ] 根据业务需求选择是否包含时间部分

## 参考文件

- `modules/workorder/views/workorder/data.ts` - plannedStart、plannedEnd
- `modules/workorder/views/workreport/data.ts` - reportTime
- `modules/equipment/views/equipment/data.ts` - purchaseDate、warrantyEndDate
- `modules/equipment/views/inspection/plan/data.ts` - nextInspectDate
- `modules/equipment/views/maintenance/plan/data.ts` - planDate
