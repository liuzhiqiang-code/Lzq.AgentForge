using FluentValidation;
using FluentValidation.Validators;
using Lzq.WorkOrder.Application.Contracts.WorkOrder.Commands;

namespace Lzq.WorkOrder.Application.Contracts.WorkOrder.Validators;

public class WorkOrderCreateCommandValidator : MasaAbstractValidator<WorkOrderCreateCommand>
{
    public WorkOrderCreateCommandValidator()
    {
        RuleFor(x => x.Code).NotNull().NotEmpty().MaximumLength(50)
            .WithMessage("工单编号不能为空且不超过50字符");
        RuleFor(x => x.ProductName).NotNull().NotEmpty().MaximumLength(200)
            .WithMessage("产品名称不能为空且不超过200字符");
        RuleFor(x => x.LineId).GreaterThan(0).WithMessage("请选择产线");
        RuleFor(x => x.ProcessId).GreaterThan(0).WithMessage("请选择工序");
        RuleFor(x => x.PlannedQty).GreaterThan(0).WithMessage("计划数量必须大于0");
        RuleFor(x => x.Priority).InclusiveBetween(1, 5).WithMessage("优先级必须在1-5之间");
        WhenNotEmpty(x => x.ProductSpec, rule => rule.MaximumLength(200));
        WhenNotEmpty(x => x.Remark, rule => rule.MaximumLength(2000));
    }
}

public class WorkOrderUpdateCommandValidator : MasaAbstractValidator<WorkOrderUpdateCommand>
{
    public WorkOrderUpdateCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("工单ID无效");
        RuleFor(x => x.ProductName).NotNull().NotEmpty().MaximumLength(200);
        RuleFor(x => x.LineId).GreaterThan(0).WithMessage("请选择产线");
        RuleFor(x => x.ProcessId).GreaterThan(0).WithMessage("请选择工序");
        RuleFor(x => x.PlannedQty).GreaterThan(0).WithMessage("计划数量必须大于0");
        RuleFor(x => x.Priority).InclusiveBetween(1, 5);
    }
}

public class WorkOrderDispatchCommandValidator : MasaAbstractValidator<WorkOrderDispatchCommand>
{
    public WorkOrderDispatchCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("工单ID无效");
    }
}

public class WorkOrderStartCommandValidator : MasaAbstractValidator<WorkOrderStartCommand>
{
    public WorkOrderStartCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("工单ID无效");
    }
}

public class WorkOrderCompleteCommandValidator : MasaAbstractValidator<WorkOrderCompleteCommand>
{
    public WorkOrderCompleteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("工单ID无效");
    }
}

public class WorkOrderCloseCommandValidator : MasaAbstractValidator<WorkOrderCloseCommand>
{
    public WorkOrderCloseCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("工单ID无效");
    }
}

public class WorkOrderPauseCommandValidator : MasaAbstractValidator<WorkOrderPauseCommand>
{
    public WorkOrderPauseCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("工单ID无效");
        WhenNotEmpty(x => x.Reason, rule => rule.MaximumLength(500));
    }
}

public class WorkOrderCancelCommandValidator : MasaAbstractValidator<WorkOrderCancelCommand>
{
    public WorkOrderCancelCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("工单ID无效");
        WhenNotEmpty(x => x.Reason, rule => rule.MaximumLength(500));
    }
}

public class WorkReportCreateCommandValidator : MasaAbstractValidator<WorkReportCreateCommand>
{
    public WorkReportCreateCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).GreaterThan(0).WithMessage("请选择工单");
        RuleFor(x => x.QualifiedQty).GreaterThanOrEqualTo(0).WithMessage("合格数量不能为负数");
        RuleFor(x => x.DefectQty).GreaterThanOrEqualTo(0).WithMessage("不良数量不能为负数");
        RuleFor(x => x.WorkHours).GreaterThan(0).WithMessage("工时必须大于0");
        RuleFor(x => x).Must(x => x.QualifiedQty + x.DefectQty > 0)
            .WithMessage("合格数量与不良数量之和必须大于0");
    }
}
