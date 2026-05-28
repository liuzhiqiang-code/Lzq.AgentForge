# MES 模块合并方案

## 一、背景

当前前端 `vue/admin/apps/web-antd/src/modules/` 下有 5 个独立的 MES 子模块：

| 模块 | 路径 | 说明 |
|------|------|------|
| `basadata` | `/basadata/*` | 基础数据（工厂/车间/产线/工序/物料/BOM） |
| `dashboard` | `/mes-dashboard/*` | 看板（设备/缺陷/完工率/产量统计） |
| `equipment` | `/equipment/*` | 设备管理（设备/点检/保养/维修） |
| `qa` | `/qa/*` | 质量管理（质检单/缺陷记录） |
| `workorder` | `/workorder/*` | 生产工单（工单/报工记录） |

后端 `MenuSeedData.cs` 已将上述 5 个模块统一合并到 MES 顶级目录下（ID=3，Path=`/mes`），子路由路径均为 `/mes/{sub}/*`，组件路径为 `#/modules/mes/views/{sub}/...`。

## 二、目标

将 5 个前端模块合并为统一的 `mes` 模块，与后端菜单体系保持一致。

## 三、最终结构

```
mes/
├── index.ts                              # 模块入口，导出 mesRoutes
├── router/
│   └── index.ts                          # 统一路由（5 个子 catalog）
├── locales/
│   └── zh-CN/
│       └── mes.json                      # 合并后的国际化文案
├── api/
│   ├── index.ts                          # API 统一入口
│   ├── factory.ts                        # 工厂 API（原 basadata）
│   ├── workshop.ts                       # 车间 API（原 basadata）
│   ├── line.ts                           # 产线 API（原 basadata）
│   ├── process.ts                        # 工序 API（原 basadata）
│   ├── material.ts                       # 物料 API（原 basadata）
│   ├── bom.ts                            # BOM API（原 basadata）
│   ├── equipment.ts                      # 设备 API（原 equipment）
│   ├── inspection.ts                     # 巡检 API（原 equipment）
│   ├── maintenance.ts                    # 保养 API（原 equipment）
│   ├── repair.ts                         # 维修 API（原 equipment）
│   ├── qcorder.ts                        # 质检单 API（原 qa）
│   ├── defect.ts                         # 缺陷 API（原 qa）
│   ├── workorder.ts                      # 工单 API（原 workorder）
│   ├── workreport.ts                     # 报工 API（原 workorder）
│   └── dashboard.ts                      # 看板 API（原 dashboard）
└── views/
    ├── basadata/
    │   ├── factory/
    │   ├── workshop/
    │   ├── line/
    │   ├── process/
    │   └── material/
    ├── dashboard/
    │   └── (原 dashboard/views/*)
    ├── equipment/
    │   ├── equipment/
    │   ├── inspection/
    │   ├── maintenance/
    │   └── repair/
    ├── qa/
    │   ├── qcorder/
    │   └── defect/
    └── workorder/
        ├── workorder/
        └── workreport/
```

## 四、变更清单

### 4.1 路由路径变更

| 原路径 | 新路径 |
|--------|--------|
| `/basadata/*` | `/mes/basadata/*` |
| `/mes-dashboard` → `/dashboard/index` | `/mes/dashboard/index` |
| `/equipment/*` | `/mes/equipment/*` |
| `/qa/*` | `/mes/qa/*` |
| `/workorder/*` | `/mes/workorder/*` |

### 4.2 导入路径变更

所有 `#/modules/{basadata|dashboard|equipment|qa|workorder}/` 替换为 `#/modules/mes/views/{sub}/` 或 `#/modules/mes/api/`（保持原有 views 子目录结构）。

### 4.3 国际化 key 变更

将各模块的顶级 key 统一到 `mes.` 前缀下：

| 原 key | 新 key |
|--------|--------|
| `basadata.*` | `mes.basadata.*` |
| `dashboard.*` | `mes.dashboard.*` |
| `equipment.*` | `mes.equipment.*` |
| `qa.*` | `mes.qa.*` |
| `workorder.*` | `mes.workorder.*` |

### 4.4 主路由注册变更

`src/router/routes/index.ts` 中，将 `basadataRoutes, dashboardRoutes, equipmentRoutes, qaRoutes, workorderRoutes` 替换为单一的 `mesRoutes`。

## 五、执行步骤

1. 创建 `mes/` 目录结构
2. 复制 5 个模块的 views、api 文件到 mes 对应子目录
3. 批量替换所有文件中的导入路径
4. 创建统一的 router/index.ts，合并 5 个子路由
5. 合并 5 个 locale JSON 为统一的 mes.json
6. 合并 API 索引文件
7. 更新主路由文件引用
8. 删除旧的 5 个模块目录
