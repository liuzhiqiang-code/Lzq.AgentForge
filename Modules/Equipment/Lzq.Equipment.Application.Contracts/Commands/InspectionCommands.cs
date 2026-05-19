using Lzq.Equipment.Domain.Enums;

namespace Lzq.Equipment.Application.Contracts.Commands;

/// <summary>
/// 创建点检计划命令
/// </summary>
public record InspectionPlanCreateCommand
{
    /// <summary>点检计划名称</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>关联设备ID</summary>
    public long EquipmentId { get; init; }

    /// <summary>关联设备编号</summary>
    public string? EquipmentCode { get; init; }

    /// <summary>关联设备名称</summary>
    public string? EquipmentName { get; init; }

    /// <summary>点检周期类型：1-每日 2-每周 3-每月 4-自定义</summary>
    public int CycleType { get; init; }

    /// <summary>点检周期值</summary>
    public int CycleValue { get; init; } = 1;

    /// <summary>下次点检日期</summary>
    public DateTime? NextInspectDate { get; init; }

    /// <summary>执行人ID</summary>
    public long? ExecutorId { get; init; }

    /// <summary>执行人名称</summary>
    public string? ExecutorName { get; init; }

    /// <summary>点检明细</summary>
    public List<InspectionItemCreateCommand>? Items { get; init; }

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}

/// <summary>
/// 点检明细创建命令
/// </summary>
public record InspectionItemCreateCommand
{
    /// <summary>点检项目名称</summary>
    public string ItemName { get; init; } = string.Empty;

    /// <summary>点检标准/要求</summary>
    public string? Standard { get; init; }

    /// <summary>点检方法</summary>
    public string? Method { get; init; }

    /// <summary>项目类型</summary>
    public int ItemType { get; init; }

    /// <summary>是否必检项</summary>
    public bool IsRequired { get; init; } = true;

    /// <summary>排序号</summary>
    public int Sort { get; init; }
}

/// <summary>
/// 执行点检命令
/// </summary>
public record InspectionExecuteCommand
{
    /// <summary>点检计划ID</summary>
    public long PlanId { get; init; }

    /// <summary>点检结果</summary>
    public InspectionResultEnum Result { get; init; }

    /// <summary>异常描述</summary>
    public string? AbnormalDesc { get; init; }

    /// <summary>是否生成报修单</summary>
    public bool CreateRepairOrder { get; init; }

    /// <summary>报修描述（如果生成报修单）</summary>
    public string? RepairDescription { get; init; }

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}
