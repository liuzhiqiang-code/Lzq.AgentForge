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

            // ======================== 基础数据目录 ========================
            new MenuEntity
            {
                Id = 3,
                Name = "Basadata",
                Path = "/basadata",
                // 已移除 Redirect，因为 TS 路由中无重定向
                Type = "catalog",
                Status = EnableStatusEnum.Enabled,
                Meta = new MenuMeta
                {
                    Order = 1000,                       // 修改为与 TS 一致的 1000
                    Icon = "ion:server-outline",        // 修改为与 TS 一致的图标
                    Title = "basadata.title"
                },
                Children = new List<MenuEntity>
                {
                    // factory 页
                    new MenuEntity
                    {
                        Id = 301,
                        Name = "BasadataFactory",
                        Path = "/basadata/factory",
                        Component = "#/modules/basadata/views/factory/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:factory",
                            Title = "basadata.factory.title"
                        }
                    },
                    // workshop 页 （顺序提前到第二位）
                    new MenuEntity
                    {
                        Id = 304,
                        Name = "BasadataWorkshop",
                        Path = "/basadata/workshop",
                        Component = "#/modules/basadata/views/workshop/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:warehouse",
                            Title = "basadata.workshop.title"
                        }
                    },
                    // line 页 （图标修正，顺序后移）
                    new MenuEntity
                    {
                        Id = 302,
                        Name = "BasadataLine",
                        Path = "/basadata/line",
                        Component = "#/modules/basadata/views/line/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:sort-variant",   // 修改为与 TS 一致
                            Title = "basadata.line.title"
                        }
                    },
                    // process 页 （图标修正，顺序最后）
                    new MenuEntity
                    {
                        Id = 303,
                        Name = "BasadataProcess",
                        Path = "/basadata/process",
                        Component = "#/modules/basadata/views/process/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:cog-sync-outline",   // 修改为与 TS 一致
                            Title = "basadata.process.title"
                        }
                    }
                }
            },

            // ======================== 设备管理目录 ========================
            new MenuEntity
            {
                Id = 4,
                Name = "Equipment",
                Path = "/equipment",
                // 已移除 Redirect，TS 中无重定向
                Type = "catalog",
                Status = EnableStatusEnum.Enabled,
                Meta = new MenuMeta
                {
                    Order = 1030,                            // 修改为与 TS 一致的排序值
                    Icon = "mdi:tools",                      // 图标一致，无需修改
                    Title = "equipment.title"
                },
                Children = new List<MenuEntity>
                {
                    // equipment 页
                    new MenuEntity
                    {
                        Id = 401,
                        Name = "EquipmentEquipment",
                        Path = "/equipment/list",            // 路径修正为 TS 中定义的 /equipment/list
                        Component = "#/modules/equipment/views/equipment/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:excavator",          // 图标修正为 mdi:excavator
                            Title = "equipment.equipment.title"
                        }
                    },
                    // inspection-plan 页
                    new MenuEntity
                    {
                        Id = 402,
                        Name = "EquipmentInspectionPlan",
                        Path = "/equipment/inspection-plan", // 路径扁平化，与 TS 一致
                        Component = "#/modules/equipment/views/inspection/plan/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:clipboard-text-search-outline", // 修正图标
                            Title = "equipment.inspectionPlan.title"
                        }
                    },
                    // inspection-record 页
                    new MenuEntity
                    {
                        Id = 403,
                        Name = "EquipmentInspectionRecord",
                        Path = "/equipment/inspection-record",
                        Component = "#/modules/equipment/views/inspection/record/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:clipboard-check-outline",       // 修正图标
                            Title = "equipment.inspectionRecord.title"
                        }
                    },
                    // maintenance-plan 页
                    new MenuEntity
                    {
                        Id = 404,
                        Name = "EquipmentMaintenancePlan",
                        Path = "/equipment/maintenance-plan",
                        Component = "#/modules/equipment/views/maintenance/plan/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:wrench-clock-outline",          // 修正图标
                            Title = "equipment.maintenancePlan.title"
                        }
                    },
                    // maintenance-record 页
                    new MenuEntity
                    {
                        Id = 405,
                        Name = "EquipmentMaintenanceRecord",
                        Path = "/equipment/maintenance-record",
                        Component = "#/modules/equipment/views/maintenance/record/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:wrench-check-outline",          // 修正图标
                            Title = "equipment.maintenanceRecord.title"
                        }
                    },
                    // repair 页
                    new MenuEntity
                    {
                        Id = 406,
                        Name = "EquipmentRepair",
                        Path = "/equipment/repair",
                        Component = "#/modules/equipment/views/repair/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:hammer-wrench",                 // 修正图标
                            Title = "equipment.repair.title"
                        }
                    }
                }
            },

            // ======================== 质量管理目录 ========================
            new MenuEntity
            {
                Id = 5,
                Name = "QA",
                Path = "/qa",
                // 已移除 Redirect，TS 中无重定向
                Type = "catalog",
                Status = EnableStatusEnum.Enabled,
                Meta = new MenuMeta
                {
                    Order = 1020,                           // 修正为 TS 中的排序值
                    Icon = "mdi:shield-check-outline",      // 修正为 TS 中的图标
                    Title = "qa.title"
                },
                Children = new List<MenuEntity>
                {
                    // qcorder 页 （顺序调整为第一）
                    new MenuEntity
                    {
                        Id = 502,
                        Name = "QaQcOrder",
                        Path = "/qa/qcorder",
                        Component = "#/modules/qa/views/qcorder/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:clipboard-check-outline",    // 与 TS 一致，无需修改
                            Title = "qa.qcorder.title"
                        }
                    },
                    // defect 页 （顺序调整为第二，图标修正）
                    new MenuEntity
                    {
                        Id = 501,
                        Name = "QaDefect",
                        Path = "/qa/defect",
                        Component = "#/modules/qa/views/defect/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:alert-circle-outline",      // 修正为 TS 中的图标
                            Title = "qa.defect.title"
                        }
                    }
                }
            },

            // ======================== 工单管理目录 ========================
            new MenuEntity
            {
                Id = 6,
                Name = "Workorder",
                Path = "/workorder",
                // 已移除 Redirect，与 TS 一致
                Type = "catalog",
                Status = EnableStatusEnum.Enabled,
                Meta = new MenuMeta
                {
                    Order = 1010,                           // 修正为 TS 中的排序值
                    Icon = "mdi:clipboard-text-outline",    // 修正为 TS 中的图标
                    Title = "workorder.title"
                },
                Children = new List<MenuEntity>
                {
                    // workorder 页
                    new MenuEntity
                    {
                        Id = 601,
                        Name = "WorkorderWorkorder",
                        Path = "/workorder/list",
                        Component = "#/modules/workorder/views/workorder/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:clipboard-list-outline", // 修正图标
                            Title = "workorder.workorder.title"
                        }
                    },
                    // workreport 页
                    new MenuEntity
                    {
                        Id = 602,
                        Name = "WorkorderWorkreport",
                        Path = "/workorder/workreport",
                        Component = "#/modules/workorder/views/workreport/list.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:clipboard-check-outline", // 修正图标
                            Title = "workorder.workreport.title"
                        }
                    }
                }
            },

            // ======================== 仪表板目录 ========================
            new MenuEntity
            {
                Id = 7,                                      // 假设下一个可用的 Id
                Name = "MESDashboard",                       // 与 TS 中 name 一致
                Path = "/mes-dashboard",
                Type = "catalog",
                Status = EnableStatusEnum.Enabled,
                Meta = new MenuMeta
                {
                    Order = 1040,
                    Icon = "mdi:view-dashboard-outline",
                    Title = "dashboard.title"
                },
                Children = new List<MenuEntity>
                {
                    // 仪表板首页
                    new MenuEntity
                    {
                        Id = 701,
                        Name = "DashboardIndex",
                        Path = "/dashboard/index",           // 与 TS 中绝对路径保持一致
                        Component = "#/modules/dashboard/views/index.vue",
                        Type = "menu",
                        Status = EnableStatusEnum.Enabled,
                        Meta = new MenuMeta
                        {
                            Icon = "mdi:monitor-dashboard",
                            Title = "dashboard.overview"
                        }
                    }
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
