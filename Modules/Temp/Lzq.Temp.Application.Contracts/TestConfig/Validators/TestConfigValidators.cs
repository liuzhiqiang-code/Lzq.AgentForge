using FluentValidation;
using Lzq.Temp.Application.Contracts.TestConfig.Commands;

namespace Lzq.Temp.Application.Contracts.TestConfig.Validators;

public class TestConfigCreateCommandValidator : AbstractValidator<TestConfigCreateCommand>
{
    public TestConfigCreateCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50).WithMessage("配置编码不能为空且不超过50字符");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("配置名称不能为空且不超过200字符");
        RuleFor(x => x.ConfigType).InclusiveBetween(1, 3).WithMessage("配置类型范围1-3");
    }
}

public class TestConfigUpdateCommandValidator : AbstractValidator<TestConfigUpdateCommand>
{
    public TestConfigUpdateCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("配置ID无效");
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50).WithMessage("配置编码不能为空且不超过50字符");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("配置名称不能为空且不超过200字符");
        RuleFor(x => x.ConfigType).InclusiveBetween(1, 3).WithMessage("配置类型范围1-3");
    }
}
