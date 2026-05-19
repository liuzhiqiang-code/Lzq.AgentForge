using FluentValidation;
using FluentValidation.Validators;
using Lzq.Equipment.Application.Contracts.Commands;

namespace Lzq.Equipment.Application.Contracts.Validators;

public class InspectionPlanCreateCommandValidator : MasaAbstractValidator<InspectionPlanCreateCommand>
{
    public InspectionPlanCreateCommandValidator()
    {
        RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(200)
            .WithMessage("点检计划名称不能为空且不超过200字符");
        RuleFor(x => x.EquipmentId).GreaterThan(0).WithMessage("请选择设备");
        RuleFor(x => x.CycleType).InclusiveBetween(1, 4).WithMessage("请选择正确的点检周期类型");
        RuleFor(x => x.CycleValue).GreaterThan(0).WithMessage("点检周期值必须大于0");
        WhenNotEmpty(x => x.Remark, rule => rule.MaximumLength(2000));
    }
}

public class InspectionItemCreateCommandValidator : MasaAbstractValidator<InspectionItemCreateCommand>
{
    public InspectionItemCreateCommandValidator()
    {
        RuleFor(x => x.ItemName).NotNull().NotEmpty().MaximumLength(200)
            .WithMessage("点检项目名称不能为空且不超过200字符");
    }
}

public class InspectionExecuteCommandValidator : MasaAbstractValidator<InspectionExecuteCommand>
{
    public InspectionExecuteCommandValidator()
    {
        RuleFor(x => x.PlanId).GreaterThan(0).WithMessage("点检计划ID无效");
        RuleFor(x => x.Result).IsInEnum().WithMessage("请选择正确的点检结果");
        WhenNotEmpty(x => x.AbnormalDesc, rule => rule.MaximumLength(2000));
        WhenNotEmpty(x => x.Remark, rule => rule.MaximumLength(2000));
    }
}
