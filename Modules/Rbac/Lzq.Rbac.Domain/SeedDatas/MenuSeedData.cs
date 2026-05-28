using Lzq.Extensions.SqlSugar.SeedData;
using Lzq.Rbac.Domain.Entities;
using Lzq.Rbac.Domain.Enums;
using Lzq.Rbac.Domain.Expands;

namespace Lzq.Rbac.Domain.SeedDatas;

public class MenuSeedData : BaseSeedData<MenuEntity>
{
    public override List<MenuEntity> GetSeedData()
    {
        // ======================== 第一步：构建树形结构 ========================
        var tree = new List<MenuEntity>
        {
            // System 目录
            new MenuEntity
            {
                Id = 1,
                Name = "System",
                Path = "/system",
                Redirect = "/system/aiChats",
                Type = "catalog",
                Status = EnableStatusEnum.Enabled,
                Meta = new MenuMeta
                {
                    Order = -1,
                    Icon = "ion:settings-outline",
                    Title = "system.title"
                },
                Children = new List<MenuEntity>
                {
                    // role 页
                    new MenuEntity
                    {
                        Id = 101,
                        Name = "SystemRole",
                        Path = "/system/role",
                        Component = "/system/role/list",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:account-group",
                            Title = "system.role.title"
                        }
                    },
                    // menu 页
                    new MenuEntity
                    {
                        Id = 102,
                        Name = "SystemMenu",
                        Path = "/system/menu",
                        Component = "/system/menu/list",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:menu",
                            Title = "system.menu.title"
                        }
                    },
                    // dept 页
                    new MenuEntity
                    {
                        Id = 103,
                        Name = "SystemDept",
                        Path = "/system/dept",
                        Component = "/system/dept/list",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "charm:organisation",
                            Title = "system.dept.title"
                        }
                    }
                }
            },

            // AI 目录
            new MenuEntity
            {
                Id = 2,
                Name = "AI",
                Path = "/ai",
                Redirect = "/ai/chats",
                Type = "catalog",
                Status = EnableStatusEnum.Enabled,
                Meta = new MenuMeta
                {
                    Order = -1,
                    Icon = "mdi:robot-happy-outline",
                    Title = "ai.title"
                },
                Children = new List<MenuEntity>
                {
                    // chats 页
                    new MenuEntity
                    {
                        Id = 201,
                        Name = "chats",
                        Path = "/ai/chats",
                        Component = "#/modules/ai/views/chats/index.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:chat-outline",          // 聊天：对话气泡
                            Title = "ai.chats.title"
                        }
                    },
                    // agentManage 页
                    new MenuEntity
                    {
                        Id = 202,
                        Name = "agentManage",
                        Path = "/ai/agentManage",
                        Component = "#/modules/ai/views/agentManage/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:robot-outline",         // 代理管理：机器人
                            Title = "ai.agentManage.title"
                        }
                    },
                    // agentSkill 页
                    new MenuEntity
                    {
                        Id = 203,
                        Name = "agentSkill",
                        Path = "/ai/agentSkill",
                        Component = "#/modules/ai/views/agentSkill/index.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:creation-outline",      // 代理技能：创意/能力
                            Title = "ai.agentSkill.title"
                        }
                    },
                    // modelConfig 页
                    new MenuEntity
                    {
                        Id = 204,
                        Name = "modelConfig",
                        Path = "/ai/modelConfig",
                        Component = "#/modules/ai/views/modelConfig/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:brain",         // 模型配置：大脑/电路
                            Title = "ai.modelConfig.title"
                        }
                    },
                    // apiKey 页
                    new MenuEntity
                    {
                        Id = 205,
                        Name = "apiKey",
                        Path = "/ai/apiKey",
                        Component = "#/modules/ai/views/apiKey/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:key-outline",           // API密钥：钥匙
                            Title = "ai.apiKey.title"
                        }
                    },
                    // 图表测试
                    new MenuEntity
                    {
                        Id = 206,
                        Name = "chartTest",
                        Path = "/ai/chartTest",
                        Component = "#/modules/ai/views/chats/chartTest.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:chat-outline",          // 聊天：对话气泡
                            Title = "ai.chartTest.title"
                        }
                    },
                }
            },

            // ======================== MES 目录（合并基础数据/设备/质量/工单/仪表板）========================
            new MenuEntity
            {
                Id = 3,
                Name = "MES",
                Path = "/mes",
                Type = "catalog",
                Status = EnableStatusEnum.Enabled,
                Meta = new MenuMeta
                {
                    Order = 1000,
                    Icon = "ion:server-outline",
                    Title = "mes.title"
                },
                Children = new List<MenuEntity>
                {
                    // ===== Basadata 子目录 =====
                    new MenuEntity
                    {
                        Id = 301,
                        Name = "MESBaseData",
                        Path = "/mes/basadata",
                        Type = "catalog",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "ion:server-outline",
                            Title = "mes.basadata.title"
                        },
                        Children = new List<MenuEntity>
                        {
                            new MenuEntity { Id = 30101, Name = "MESFactory", Path = "/mes/basadata/factory", Component = "#/modules/mes/views/basadata/factory/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:factory", Title = "mes.basadata.factory.title" } },
                            new MenuEntity { Id = 30102, Name = "MESWorkshop", Path = "/mes/basadata/workshop", Component = "#/modules/mes/views/basadata/workshop/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:warehouse", Title = "mes.basadata.workshop.title" } },
                            new MenuEntity { Id = 30103, Name = "MESLine", Path = "/mes/basadata/line", Component = "#/modules/mes/views/basadata/line/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:sort-variant", Title = "mes.basadata.line.title" } },
                            new MenuEntity { Id = 30104, Name = "MESProcess", Path = "/mes/basadata/process", Component = "#/modules/mes/views/basadata/process/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:cog-sync-outline", Title = "mes.basadata.process.title" } },
                            new MenuEntity { Id = 30105, Name = "MESMaterial", Path = "/mes/basadata/material", Component = "#/modules/mes/views/basadata/material/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:package-variant-closed", Title = "mes.basadata.material.title" } },
                        }
                    },
                    // ===== WorkOrder 子目录 =====
                    new MenuEntity
                    {
                        Id = 302,
                        Name = "MESWorkOrder",
                        Path = "/mes/workorder",
                        Type = "catalog",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:clipboard-text-outline",
                            Title = "mes.workorder.title"
                        },
                        Children = new List<MenuEntity>
                        {
                            new MenuEntity { Id = 30201, Name = "MESWorkOrderList", Path = "/mes/workorder/list", Component = "#/modules/mes/views/workorder/workorder/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:clipboard-list-outline", Title = "mes.workorder.workorder.title" } },
                            new MenuEntity { Id = 30202, Name = "MESWorkReport", Path = "/mes/workorder/workreport", Component = "#/modules/mes/views/workorder/workreport/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:clipboard-check-outline", Title = "mes.workorder.workreport.title" } },
                        }
                    },
                    // ===== QA 子目录 =====
                    new MenuEntity
                    {
                        Id = 303,
                        Name = "MESQA",
                        Path = "/mes/qa",
                        Type = "catalog",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:shield-check-outline",
                            Title = "mes.qa.title"
                        },
                        Children = new List<MenuEntity>
                        {
                            new MenuEntity { Id = 30301, Name = "MESQCOrder", Path = "/mes/qa/qcorder", Component = "#/modules/mes/views/qa/qcorder/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:clipboard-check-outline", Title = "mes.qa.qcorder.title" } },
                            new MenuEntity { Id = 30302, Name = "MESDefect", Path = "/mes/qa/defect", Component = "#/modules/mes/views/qa/defect/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:alert-circle-outline", Title = "mes.qa.defect.title" } },
                        }
                    },
                    // ===== Equipment 子目录 =====
                    new MenuEntity
                    {
                        Id = 304,
                        Name = "MESEquipment",
                        Path = "/mes/equipment",
                        Type = "catalog",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:tools",
                            Title = "mes.equipment.title"
                        },
                        Children = new List<MenuEntity>
                        {
                            new MenuEntity { Id = 30401, Name = "MESEquipmentList", Path = "/mes/equipment/list", Component = "#/modules/mes/views/equipment/equipment/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:excavator", Title = "mes.equipment.equipment.title" } },
                            new MenuEntity { Id = 30402, Name = "MESInspectionPlan", Path = "/mes/equipment/inspection-plan", Component = "#/modules/mes/views/equipment/inspection/plan/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:clipboard-text-search-outline", Title = "mes.equipment.inspectionPlan.title" } },
                            new MenuEntity { Id = 30403, Name = "MESInspectionRecord", Path = "/mes/equipment/inspection-record", Component = "#/modules/mes/views/equipment/inspection/record/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:clipboard-check-outline", Title = "mes.equipment.inspectionRecord.title" } },
                            new MenuEntity { Id = 30404, Name = "MESMaintenancePlan", Path = "/mes/equipment/maintenance-plan", Component = "#/modules/mes/views/equipment/maintenance/plan/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:wrench-clock-outline", Title = "mes.equipment.maintenancePlan.title" } },
                            new MenuEntity { Id = 30405, Name = "MESMaintenanceRecord", Path = "/mes/equipment/maintenance-record", Component = "#/modules/mes/views/equipment/maintenance/record/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:wrench-check-outline", Title = "mes.equipment.maintenanceRecord.title" } },
                            new MenuEntity { Id = 30406, Name = "MESRepair", Path = "/mes/equipment/repair", Component = "#/modules/mes/views/equipment/repair/list.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:hammer-wrench", Title = "mes.equipment.repair.title" } },
                        }
                    },
                    // ===== Dashboard 子目录 =====
                    new MenuEntity
                    {
                        Id = 305,
                        Name = "MESDashboard",
                        Path = "/mes/dashboard",
                        Type = "catalog",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:view-dashboard-outline",
                            Title = "mes.dashboard.title"
                        },
                        Children = new List<MenuEntity>
                        {
                            new MenuEntity { Id = 30501, Name = "MESDashboardIndex", Path = "/mes/dashboard/index", Component = "#/modules/mes/views/dashboard/index.vue", Type = "menu", Status = EnableStatusEnum.Enabled, Meta = new MenuMeta { Icon = "mdi:monitor-dashboard", Title = "mes.dashboard.overview" } },
                        }
                    },
                }
            }
        };

        // ======================== 第二步：平铺并返回 ========================
        return Flatten(tree, null);
    }

    /// <summary>
    /// 递归平铺树形菜单，自动设置正确的 Pid
    /// </summary>
    private static List<MenuEntity> Flatten(List<MenuEntity> nodes, long? parentId)
    {
        var result = new List<MenuEntity>();
        foreach (var node in nodes)
        {
            node.Pid = parentId;               // 设置父级 ID
            var children = node.Children;      // 暂存子节点
            node.Children = null;              // 清除 Children（数据库无需存储）
            result.Add(node);
            if (children?.Count > 0)
                result.AddRange(Flatten(children, node.Id));
        }
        return result;
    }
}
