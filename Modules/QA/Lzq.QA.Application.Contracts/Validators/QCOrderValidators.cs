using FluentValidation;
using FluentValidation.Validators;
using Lzq.QA.Application.Contracts.Commands;

namespace Lzq.QA.Application.Contracts.Validators;

public class QCOrderCreateCommandValidator : MasaAbstractValidator<QCOrderCreateCommand>
{
    public QCOrderCreateCommandValidator()
    {
        RuleFor(x => x.QCType).IsInEnum().WithMessage("请选择正确的质检类型");
        RuleFor(x => x.SubmitQty).GreaterThan(0).WithMessage("送检数量必须大于0");
        When(x => x.QCType == Domain.Enums.QCTypeEnum.IQC, () =>
        {
            RuleFor(x => x.SupplierId).GreaterThan(0).WithMessage("来料检验必须选择供应商");
        });
        WhenNotEmpty(x => x.ProductName, rule => rule.MaximumLength(200));
        WhenNotEmpty(x => x.ProductSpec, rule => rule.MaximumLength(200));
        WhenNotEmpty(x => x.RefCode, rule => rule.MaximumLength(100));
        WhenNotEmpty(x => x.BatchNo, rule => rule.MaximumLength(100));
        WhenNotEmpty(x => x.QCStandard, rule => rule.MaximumLength(1000));
        WhenNotEmpty(x => x.Remark, rule => rule.MaximumLength(2000));
    }
}

public class QCOrderUpdateCommandValidator : MasaAbstractValidator<QCOrderUpdateCommand>
{
    public QCOrderUpdateCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("质检单ID无效");
        RuleFor(x => x.SubmitQty).GreaterThan(0).WithMessage("送检数量必须大于0");
        WhenNotEmpty(x => x.ProductName, rule => rule.MaximumLength(200));
        WhenNotEmpty(x => x.ProductSpec, rule => rule.MaximumLength(200));
        WhenNotEmpty(x => x.BatchNo, rule => rule.MaximumLength(100));
        WhenNotEmpty(x => x.QCStandard, rule => rule.MaximumLength(1000));
    }
}

public class QCOrderSubmitInspectCommandValidator : MasaAbstractValidator<QCOrderSubmitInspectCommand>
{
    public QCOrderSubmitInspectCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("质检单ID无效");
        RuleFor(x => x.Items).NotNull().NotEmpty().WithMessage("检验明细不能为空");
        RuleForEach(x => x.Items).SetValidator(new QCOrderItemInspectCommandValidator());
    }
}

public class QCOrderItemInspectCommandValidator : MasaAbstractValidator<QCOrderItemInspectCommand>
{
    public QCOrderItemInspectCommandValidator()
    {
        RuleFor(x => x.ItemName).NotNull().NotEmpty().MaximumLength(200).WithMessage("检验项目名称不能为空且不超过200字符");
        RuleFor(x => x.SampleQty).GreaterThan(0).WithMessage("抽样数量必须大于0");
        RuleFor(x => x.Result).IsInEnum().WithMessage("请选择正确的检验结果");
        RuleFor(x => x.QualifiedQty).GreaterThanOrEqualTo(0).WithMessage("合格数量不能为负数");
        RuleFor(x => x.UnqualifiedQty).GreaterThanOrEqualTo(0).WithMessage("不合格数量不能为负数");
    }
}

public class QCOrderJudgeCommandValidator : MasaAbstractValidator<QCOrderJudgeCommand>
{
    public QCOrderJudgeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("质检单ID无效");
        RuleFor(x => x.Result).Must(x => x == Domain.Enums.QCOrderStatusEnum.Qualified || x == Domain.Enums.QCOrderStatusEnum.Unqualified)
            .WithMessage("判定结果必须是合格或不合格");
        RuleFor(x => x.QualifiedQty).GreaterThanOrEqualTo(0).WithMessage("合格数量不能为负数");
        RuleFor(x => x.UnqualifiedQty).GreaterThanOrEqualTo(0).WithMessage("不合格数量不能为负数");
        WhenNotEmpty(x => x.Conclusion, rule => rule.MaximumLength(2000));
    }
}

public class QCOrderCancelCommandValidator : MasaAbstractValidator<QCOrderCancelCommand>
{
    public QCOrderCancelCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("质检单ID无效");
        WhenNotEmpty(x => x.Reason, rule => rule.MaximumLength(500));
    }
}
