namespace Lzq.Dashboard.Domain.Consts;

/// <summary>
/// Dashboard 模块 Redis Key 常量定义
/// </summary>
public class RedisKeys
{
    /// <summary>
    /// 产量统计汇总缓存（按产线），TTL 5分钟
    /// 格式化参数：lineId（可为null，传0表示全部）
    /// </summary>
    public const string ProductionSummary = "Kanban:ProductionSummary:{0}";

    /// <summary>
    /// 不良率趋势汇总缓存（按产线），TTL 5分钟
    /// 格式化参数：lineId（可为null，传0表示全部）
    /// </summary>
    public const string DefectSummary = "Kanban:DefectSummary:{0}";

    /// <summary>
    /// 工单完成率汇总缓存（按产线），TTL 5分钟
    /// 格式化参数：lineId（可为null，传0表示全部）
    /// </summary>
    public const string WorkOrderSummary = "Kanban:WorkOrderSummary:{0}";

    /// <summary>
    /// 设备状态概览缓存，TTL 5分钟
    /// </summary>
    public const string EquipmentOverview = "Kanban:EquipmentOverview";

    /// <summary>
    /// 看板配置列表缓存（按配置类型），TTL 1小时
    /// 格式化参数：configType（可为null，传0表示全部）
    /// </summary>
    public const string ConfigList = "Kanban:ConfigList:{0}";

    /// <summary>
    /// 看板配置详情缓存，TTL 1小时
    /// 格式化参数：id
    /// </summary>
    public const string Config = "Kanban:Config:{0}";

    /// <summary>
    /// 全部配置列表缓存（configType=0）
    /// </summary>
    public const string ConfigListAll = "Kanban:ConfigList:0";
}
