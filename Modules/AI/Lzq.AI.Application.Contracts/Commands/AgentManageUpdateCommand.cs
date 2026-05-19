using FluentValidation;
using FluentValidation.Validators;

namespace Lzq.AI.Application.Contracts.Commands;

public record AgentManageUpdateCommand
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 智能体名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 智能体描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 智能体提示词
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// 温度控制
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// 最大输出token数
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// 核采样，控制多样性
    /// </summary>
    public float? TopP { get; set; }

    /// <summary>
    /// 减少重复token
    /// </summary>
    public float? FrequencyPenalty { get; set; }

    /// <summary>
    /// 减少已出现token
    /// </summary>
    public float? PresencePenalty { get; set; }

    /// <summary>
    /// 绑定工具
    /// </summary>
    public List<SkillMethodEntry>? SelectedSkills { get; set; }

}
public class AIAgentUpdateCommandValidator : MasaAbstractValidator<AgentManageUpdateCommand>
{
    public AIAgentUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID必须大于0");
    }
}