using FluentValidation;
using FluentValidation.Validators;
using Lzq.QA.Application.Contracts.Commands;

namespace Lzq.QA.Application.Contracts.Validators;

public class DefectRecordCreateCommandValidator : MasaAbstractValidator<DefectRecordCreateCommand>
{
    public DefectRecordCreateCommandValidator()
    {
        RuleFor(x => x.DefectQty).GreaterThan(0).WithMessage("不良品数量必须大于0");
        RuleFor(x => x.DefectCode).NotNull().NotEmpty().MaximumLength(50).WithMessage("不合格代码不能为空且不超过50字符");
        RuleFor(x => x.DefectDesc).NotNull().NotEmpty().MaximumLength(1000).WithMessage("不合格描述不能为空且不超过1000字符");
        WhenNotEmpty(x => x.ProductName, rule => rule.MaximumLength(200));
        WhenNotEmpty(x => x.ProductSpec, rule => rule.MaximumLength(200));
        WhenNotEmpty(x => x.BatchNo, rule => rule.MaximumLength(100));
        WhenNotEmpty(x => x.Remark, rule => rule.MaximumLength(2000));
    }
}

public class DefectRecordHandleCommandValidator : MasaAbstractValidator<DefectRecordHandleCommand>
{
    public DefectRecordHandleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("不良品记录ID无效");
        RuleFor(x => x.HandlingType).IsInEnum().WithMessage("请选择正确的处理方式");
        WhenNotEmpty(x => x.HandlingRemark, rule => rule.MaximumLength(2000));
    }
}

public class DefectRecordReviewCommandValidator : MasaAbstractValidator<DefectRecordReviewCommand>
{
    public DefectRecordReviewCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("不良品记录ID无效");
        RuleFor(x => x.ReviewResult).NotNull().NotEmpty().MaximumLength(1000).WithMessage("评审结果不能为空且不超过1000字符");
    }
}
