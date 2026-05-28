namespace Lzq.MES.Domain.Consts;

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

    // ══════════════════════════════════════════
    // BaseData 模块 Key
    // ══════════════════════════════════════════

    /// <summary>
    /// 工厂树缓存（四级结构：工厂→车间→产线→工序），TTL 2小时
    /// </summary>
    public const string FactoryTree = "Factory:Tree";

    /// <summary>
    /// 物料类型树缓存，TTL 2小时
    /// </summary>
    public const string MaterialTypeTree = "MaterialType:Tree";

    /// <summary>
    /// 物料下拉列表缓存，TTL 2小时
    /// </summary>
    public const string MaterialSelectList = "Material:SelectList";

    /// <summary>
    /// 计量单位列表缓存，TTL 2小时
    /// </summary>
    public const string UnitOfMeasureList = "UnitOfMeasure:List";

    // ══════════════════════════════════════════
    // Equipment 模块 Key
    // ══════════════════════════════════════════

    /// <summary>
    /// 设备详情缓存，TTL 1小时
    /// 格式化参数：id
    /// </summary>
    public const string EquipmentGet = "Equipment:Get:{0}";

    /// <summary>
    /// 设备统计缓存，TTL 10分钟
    /// </summary>
    public const string EquipmentStatistics = "Equipment:Statistics";

    // ══════════════════════════════════════════
    // QA 模块 Key
    // ══════════════════════════════════════════

    /// <summary>
    /// 质检单详情缓存，TTL 30分钟
    /// 格式化参数：id
    /// </summary>
    public const string QCOrderGet = "QCOrder:Get:{0}";

    /// <summary>
    /// 质检单检验明细缓存，TTL 30分钟
    /// 格式化参数：qcOrderId
    /// </summary>
    public const string QCOrderItems = "QCOrder:Items:{0}";
}
