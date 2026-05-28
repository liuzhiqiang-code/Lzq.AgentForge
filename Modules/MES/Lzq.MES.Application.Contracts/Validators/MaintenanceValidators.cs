using FluentValidation;
using FluentValidation.Validators;
using Lzq.MES.Application.Contracts.Commands;

namespace Lzq.MES.Application.Contracts.Validators;

public class RepairOrderCreateCommandValidator : MasaAbstractValidator<RepairOrderCreateCommand>
{
    public RepairOrderCreateCommandValidator()
    {
        RuleFor(x => x.EquipmentId).GreaterThan(0).WithMessage("请选择设备");
        RuleFor(x => x.RepairType).InclusiveBetween(1, 4).WithMessage("请选择正确的报修类型");
        RuleFor(x => x.Description).NotNull().NotEmpty().MaximumLength(2000)
            .WithMessage("报修描述不能为空且不超过2000字符");
        RuleFor(x => x.Priority).IsInEnum().WithMessage("请选择正确的优先级");
        WhenNotEmpty(x => x.Remark, rule => rule.MaximumLength(2000));
    }
}

public class RepairAssignCommandValidator : MasaAbstractValidator<RepairAssignCommand>
{
    public RepairAssignCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("报修单ID无效");
        RuleFor(x => x.RepairUserId).GreaterThan(0).WithMessage("请选择维修人员");
        RuleFor(x => x.RepairUserName).NotNull().NotEmpty().MaximumLength(100)
            .WithMessage("维修人员名称不能为空");
    }
}

public class RepairStartCommandValidator : MasaAbstractValidator<RepairStartCommand>
{
    public RepairStartCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("报修单ID无效");
    }
}

public class RepairCompleteCommandValidator : MasaAbstractValidator<RepairCompleteCommand>
{
    public RepairCompleteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("报修单ID无效");
        RuleFor(x => x.WorkHours).GreaterThanOrEqualTo(0).WithMessage("维修工时不能为负数");
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0).WithMessage("维修费用不能为负数");
        WhenNotEmpty(x => x.FaultReason, rule => rule.MaximumLength(2000));
        WhenNotEmpty(x => x.RepairProcess, rule => rule.MaximumLength(4000));
    }
}

public class RepairAcceptCommandValidator : MasaAbstractValidator<RepairAcceptCommand>
{
    public RepairAcceptCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("报修单ID无效");
        WhenNotEmpty(x => x.AcceptComment, rule => rule.MaximumLength(1000));
    }
}
