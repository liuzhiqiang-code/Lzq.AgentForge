# 模式 4：详情 + Tab 切换

> 适用场景：多维度详情展示
> 参考：playground 中的 details 模式

## 实现方式

使用 Ant Design Vue 的 `Tabs` 组件或 Vben 的布局模式实现多 Tab 详情：

```vue
<template>
  <Page>
    <Tabs v-model:activeKey="activeTab">
      <TabPane key="basic" :tab="$t('common.basicInfo')">
        <BasicInfo :data="detail" />
      </TabPane>
      <TabPane key="records" :tab="$t('common.relatedRecords')">
        <RelatedRecords :parent-id="id" />
      </TabPane>
    </Tabs>
  </Page>
</template>
```

## 关键点
- 每个 Tab 的内容拆分为独立的子组件
- 数据获取通常在父组件完成，通过 props 传递给子组件
- 涉及关联数据的 Tab 可自行请求数据（传入 parentId）
