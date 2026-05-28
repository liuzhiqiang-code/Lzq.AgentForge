# System 模块迁移方案

## 背景

前端 RBAC 管理页面（角色/菜单/部门）当前位于传统 `views/system/` 目录，API 位于全局 `api/system/`，不符合模块化架构。需要迁移至 `modules/system/` 下，与 AI/MES 模块保持一致。

## 目标

将 `views/system/` 内容迁移到 `modules/system/`，后端菜单种子数据同步更新 Component 路径。

## 依赖链分析

```
#/api (src/api/index.ts)
  → export * from './system' (api/system/index.ts)
    → export * from './role' | './menu' | './dept'
```

`#/api` barrel 被 `store/auth.ts`、`router/access.ts`、`views/_core/profile/` 广泛使用，不能直接删除 `api/system/index.ts`，需改为桥接重导出。

## 变更清单

### 1. 后端 MenuSeedData.cs

| 原 Component | 新 Component |
|---|---|
| `/system/role/list` | `#/modules/system/views/role/list.vue` |
| `/system/menu/list` | `#/modules/system/views/menu/list.vue` |
| `/system/dept/list` | `#/modules/system/views/dept/list.vue` |

### 2. 前端文件搬迁

| 操作 | 原路径 | 新路径 |
|------|--------|--------|
| 移动 | `views/system/{role,menu,dept}/*` (9 文件) | `modules/system/views/{role,menu,dept}/*` |
| 复制 | `api/system/{role,menu,dept}.ts` | `modules/system/api/{role,menu,dept}.ts` |
| 复制 | `locales/langs/zh-CN/system.json` | `modules/system/locales/zh-CN/system.json` |
| 删除 | `api/system/modeling.ts` | — |
| 删除 | `api/system/{role,menu,dept}.ts` | — (桥接替代) |
| 新建 | `modules/system/index.ts` | 模块入口 |
| 新建 | `modules/system/router/index.ts` | 路由定义 |
| 新建 | `modules/system/api/index.ts` | API barrel |

### 3. Import 路径替换

| 原 | 新 |
|----|-----|
| `from '#/api/system/role'` | `from '#/modules/system/api/role'` |
| `from '#/api/system/menu'` | `from '#/modules/system/api/menu'` |
| `from '#/api/system/dept'` | `from '#/modules/system/api/dept'` |
| `from '#/api'` (role 文件) | `from '#/modules/system/api/role'` |

### 4. api/system/index.ts 桥接

```ts
export * from '#/modules/system/api';
```

### 5. 主路由注册

```ts
import { systemRoutes } from '#/modules/system';
```

### 6. 清理

- 删除 `views/system/` 目录
- 删除 `api/system/role.ts`、`api/system/menu.ts`、`api/system/dept.ts`、`api/system/modeling.ts`
- 删除 `locales/langs/zh-CN/system.json`
