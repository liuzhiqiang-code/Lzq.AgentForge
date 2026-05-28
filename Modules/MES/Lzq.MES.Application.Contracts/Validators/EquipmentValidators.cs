using FluentValidation;
using FluentValidation.Validators;
using Lzq.MES.Application.Contracts.Commands;

namespace Lzq.MES.Application.Contracts.Validators;

public class EquipmentCreateCommandValidator : MasaAbstractValidator<EquipmentCreateCommand>
{
    public EquipmentCreateCommandValidator()
    {
        RuleFor(x => x.Code).NotNull().NotEmpty().MaximumLength(50)
            .WithMessage("设备编号不能为空且不超过50字符");
        RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(200)
            .WithMessage("设备名称不能为空且不超过200字符");
        RuleFor(x => x.EquipmentType).IsInEnum().WithMessage("请选择正确的设备类型");
        WhenNotEmpty(x => x.Spec, rule => rule.MaximumLength(200));
        WhenNotEmpty(x => x.Brand, rule => rule.MaximumLength(200));
        WhenNotEmpty(x => x.Supplier, rule => rule.MaximumLength(200));
        WhenNotEmpty(x => x.Location, rule => rule.MaximumLength(200));
        WhenNotEmpty(x => x.Remark, rule => rule.MaximumLength(2000));
    }
}

public class EquipmentUpdateCommandValidator : MasaAbstractValidator<EquipmentUpdateCommand>
{
    public EquipmentUpdateCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("设备ID无效");
        RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(200);
        WhenNotEmpty(x => x.Spec, rule => rule.MaximumLength(200));
        WhenNotEmpty(x => x.Brand, rule => rule.MaximumLength(200));
        WhenNotEmpty(x => x.Location, rule => rule.MaximumLength(200));
        WhenNotEmpty(x => x.Remark, rule => rule.MaximumLength(2000));
    }
}

public class EquipmentUpdateStatusCommandValidator : MasaAbstractValidator<EquipmentUpdateStatusCommand>
{
    public EquipmentUpdateStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("设备ID无效");
        RuleFor(x => x.Status).IsInEnum().WithMessage("请选择正确的设备状态");
    }
}
