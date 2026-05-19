---
name: vben-frontend-dev
description: Vben Admin 前端开发规范。涵盖 Vue 3 + TypeScript + Vben Admin (web-antd) 技术栈下的模块化开发、页面模式、组件用法、代码审查。当需要创建/修改前端页面、使用 Vben 组件、或审查前端代码时使用此 skill。
license: 内部项目使用
---

# Vben Admin 前端开发 Skill

> **角色**: Vben Admin 前端开发工程师
> **技术栈**: Vue 3 + TypeScript + Vben Admin (web-antd)
> **项目路径**: `code/vue/admin/apps/web-antd/`

## 核心原则

### 框架约束（不可突破）
- 技术栈锁定：Vue 3 + TypeScript + Vben Admin (web-antd)
- 组件来源：仅限 `@vben/*` 和 `ant-design-vue`
- 目录结构：遵循 Vben Admin 标准模块化目录
- 三条红线：❌ 禁止 `npm install` 新包 | ❌ 禁止绕过框架路由/权限/请求封装 | ❌ 禁止违背 Ant Design 规范的自创交互

### 业务自由（允许发挥）
页面布局组合、组件交互逻辑、数据处理与状态管理、符合 Ant Design 规范的 UI 编排

## 使用流程

1. **识别页面类型**：确定属于哪种页面模式（树形表格/搜索分页/抽屉编辑/详情Tab）
2. **读取对应模式**：Read `patterns/` 下对应文件获取完整代码模板
3. **查阅组件用法**：需要特定组件时 Read `components/` 下对应文件
4. **执行审查清单**：完成后对照 `checklist.md` 逐项自检
5. **排查已知问题**：遇到异常先查 `known-issues.md`

## 页面模式 → `patterns/`

| 模式 | 文件 | 适用场景 | 典型提问 |
|------|------|---------|---------|
| 树形表格 + 弹窗 | `patterns/tree-table-modal.md` | 部门管理、分类管理 | "创建树形结构页面" |
| 搜索 + 表格 + 分页 | `patterns/search-table-page.md` | 角色、用户、工单列表 | "创建带搜索的列表页" |
| 表格 + 抽屉 | `patterns/table-drawer.md` | 复杂编辑、树形选择 | "创建抽屉编辑表单" |
| 详情 + Tab | `patterns/detail-tabs.md` | 多维度详情展示 | "创建多Tab详情页" |

## 组件用法 → `components/`

| 组件 | 文件 | 说明 | 典型提问 |
|------|------|------|---------|
| ApiSelect | `components/api-select.md` | 远程下拉框 + 反模式 | "怎么做远程下拉框" |
| ApiTreeSelect | `components/api-tree-select.md` | 远程树形选择 | "怎么做树形下拉框" |
| DatePicker | `components/date-picker.md` | 日期选择标准配置 | "日期选择器怎么配置" |
| 表单联动 | `components/linked-select.md` | 级联选择（工厂→车间→产线） | "怎么做级联下拉框" |
| Upload | `components/upload.md` | 文件上传 | "怎么做文件上传" |
| IconPicker | `components/icon-picker.md` | 图标选择器 | "怎么做图标选择" |
| 组件 API 速查 | `components/component-api.md` | Modal/Drawer/Form/Grid API | "useVbenModal 怎么用" |

## 开发命令

### vben-create-page
创建新业务页面。流程：选择模式 → Read 对应 patterns/ 文件 → 按模板编写代码 → 对照片 `checklist.md` 自检

### vben-modify-page
修改现有页面。先完整阅读现有代码，保持风格一致，修改后执行审查清单

### vben-add-component
提取可复用业务组件。放在 `src/views/{module}/components/`，必须含 Props/Emits 类型定义

## 开发流程

```
理解需求 → 选择模式 → Read patterns/ → 参考组件用法 → 编写代码 → checklist.md 自检
```

## 审查与问题排查

- **代码审查清单 + 错误处理规范** → `checklist.md`
- **已知问题**（编码乱码、API路径、Switch 0/1、class:w-full 等）→ `known-issues.md`
