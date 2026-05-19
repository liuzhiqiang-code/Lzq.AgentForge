namespace Lzq.Rbac.Application.Contracts.Commands;

public record RegisterCommand
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { get; init; }
    /// <summary>
    /// 密码
    /// </summary>
    public string? Password { get; init; }
}
