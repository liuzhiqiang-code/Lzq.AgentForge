# 已知问题与注意事项

## 1. UTF-8 编码损坏问题

AI Agent 生成的 Vue/TS 文件可能出现"合法 UTF-8 但中文乱码"的问题。这不是文件编码错误，而是 UTF-8 字节流被中间编码步骤损坏。

**典型乱码识别**：`导入`→`瀵煎叆`、`暗黑模式`→`鏆楅粦妯″紡`、`新建对话`→`鏂板缓瀵硅瘽`

**修复方法**：直接用 Write 工具写入正确中文（不要尝试 Python 编码转换，会失败）

**检测方法**：使用 `file` 命令检查编码（不要用 `cat -v`，会误报 UTF-8 中文）
- ✓ UTF-8 / ASCII / Unicode text - 编码正确
- ✗ GBK / ISO-8859 - 需要转换

## 2. API 路径规范

- `requestClient` 的 `baseURL` 已包含 `/api/v1`，前端调用时**不要**再加此前缀
- ❌ `/api/v1/mes/work-order/...`
- ✅ `/mes/work-order/...`
- GET 请求参数用 `{ params }` 传递

## 3. Switch 组件：true/false vs 0/1

前端 Switch 默认使用 true/false，后端使用 0/1 整数枚举（EnableStatusEnum: 0=Disabled, 1=Enabled）。

必须为 Switch 组件添加：
```typescript
componentProps: {
  checkedValue: 1,
  unCheckedValue: 0,
}
```

已修复模块（2026-05-16）：basadata(line/process/workshop)、ai(aiModelConfig/apiKey)、equipment(inspection/plan, maintenance/plan)

## 4. formApi.updateSchema() vs ApiSelect 选择

| 场景 | 用什么 |
|------|--------|
| 简单的独立下拉框 | ApiSelect 的 `api` 属性 ✅ |
| 需要级联加载（前一项变化后加载后一项） | `formApi.updateSchema()` |
| 需要条件加载 | `formApi.updateSchema()` |
| 需要手动控制加载时机 | `formApi.updateSchema()` |

## 5. 已知参考问题

1. **@vben/common-ui** 的 `useVbenForm` 返回的 `Form` 是一个组件引用（可在 template 中使用 `<Form />`）
2. **proxyConfig.ajax.query** 的回调参数是 `({ page, sort, filters, formValues })`
3. **`export default routes`** 是路由模块的标准导出方式
4. **图标**使用 Iconify 名称（如 `'ion:settings-outline'`），通过 `@vben/icons` 渲染
5. **别名路径**：`#/` 映射到 `src/`（通过 package.json imports 配置）
6. **请求响应**：`requestClient` 默认返回 `responseReturn: 'data'`

## 6. 模块实际路径

- 前端模块在 `apps/web-antd/src/modules/` 下
- 多语言必须使用 `locales/zh-CN/{module}.json` 目录结构
- 启动命令：`pnpm dev:antd`（从根目录）
- 登录凭据：`admin` / `123456`

## 7. class: 'w-full' 批量规范

所有表单组件必须配置 `class: 'w-full'`（2026-05-16 已完成 17 个文件批量添加）：

- 无 componentProps → 添加 `componentProps: { class: 'w-full' }`
- 有 componentProps 无 class → 在对象内添加 `class: 'w-full'`
- 动态 componentProps → 在返回对象中添加 `class: 'w-full'`
- 已有 class: 'w-full' → 跳过

## 8. 多语言文本规范

- 使用 `$t('module.field')` 而非硬编码中文
- 确认对话框 `okText` 使用 `$t('common.confirm')`（非 `$t('common.ok')`）

## 9. 框架文档速查

| 文档 | 路径 |
|------|------|
| 基础概念 | `docs/src/guide/essentials/concept.md` |
| 路由和菜单 | `docs/src/guide/essentials/route.md` |
| 开发 | `docs/src/guide/essentials/development.md` |
| 构建 | `docs/src/guide/essentials/build.md` |
| 样式 | `docs/src/guide/essentials/styles.md` |
| 服务端交互 | `docs/src/guide/essentials/server.md` |
| 图标 | `docs/src/guide/essentials/icons.md` |
| 权限 | `docs/src/guide/in-depth/access.md` |
| 布局 | `docs/src/guide/in-depth/layout.md` |
| 多语言 | `docs/src/guide/in-depth/locale.md` |
| 主题 | `docs/src/guide/in-depth/theme.md` |
| 登录 | `docs/src/guide/in-depth/login.md` |
| 常见问题 | `docs/src/guide/other/faq.md` |

## 10. 演示参考（playground）

| 模式 | 参考文件 |
|------|---------|
| 树形表格 + 弹窗 | `views/system/dept/list.vue` |
| 搜索表格 + 抽屉 | `views/system/role/list.vue` |
| 表格 CRUD + 弹窗 | `views/system/menu/list.vue` |
| 仪表盘 | `views/dashboard/analytics/index.vue` |
| 多步表单 | `views/examples/form/` |
| VxeTable 进阶 | `views/examples/vxe-table/` |
