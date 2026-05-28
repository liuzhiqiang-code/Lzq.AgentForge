using FluentValidation;
using FluentValidation.Validators;
using Lzq.MES.Application.Contracts.Commands;

namespace Lzq.MES.Application.Contracts.Validators;

public class InspectionPlanCreateCommandValidator : MasaAbstractValidator<InspectionPlanCreateCommand>
{
    public InspectionPlanCreateCommandValidator()
    {
        RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(200)
            .WithMessage("鐐规璁″垝鍚嶇О涓嶈兘涓虹┖涓斾笉瓒呰繃200瀛楃");
        RuleFor(x => x.EquipmentId).GreaterThan(0).WithMessage("璇烽€夋嫨璁惧");
        RuleFor(x => x.CycleType).InclusiveBetween(1, 4).WithMessage("璇烽€夋嫨姝ｇ‘鐨勭偣妫€鍛ㄦ湡绫诲瀷");
        RuleFor(x => x.CycleValue).GreaterThan(0).WithMessage("鐐规鍛ㄦ湡鍊煎繀椤诲ぇ浜?");
        WhenNotEmpty(x => x.Remark, rule => rule.MaximumLength(2000));
    }
}

public class InspectionItemCreateCommandValidator : MasaAbstractValidator<InspectionItemCreateCommand>
{
    public InspectionItemCreateCommandValidator()
    {
        RuleFor(x => x.ItemName).NotNull().NotEmpty().MaximumLength(200)
            .WithMessage("鐐规椤圭洰鍚嶇О涓嶈兘涓虹┖涓斾笉瓒呰繃200瀛楃");
    }
}

public class InspectionExecuteCommandValidator : MasaAbstractValidator<InspectionExecuteCommand>
{
    public InspectionExecuteCommandValidator()
    {
        RuleFor(x => x.PlanId).GreaterThan(0).WithMessage("鐐规璁″垝ID鏃犳晥");
        RuleFor(x => x.Result).IsInEnum().WithMessage("璇烽€夋嫨姝ｇ‘鐨勭偣妫€缁撴灉");
        WhenNotEmpty(x => x.AbnormalDesc, rule => rule.MaximumLength(2000));
        WhenNotEmpty(x => x.Remark, rule => rule.MaximumLength(2000));
    }
}

