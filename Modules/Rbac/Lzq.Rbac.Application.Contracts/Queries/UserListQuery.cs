/*
 * @author : liuzhiqiang
 * @date : 2026-1-1
 * @desc : user
 */
namespace Lzq.Rbac.Application.Contracts.Queries;

public record UserListQuery
{
    public long? Id { get; set; }

    public string? UserName { get; set; }

    public string? Surname { get; set; }

    public string? GivenName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int? Age { get; set; }

    public int? Sex { get; set; }

    public string? Remark { get; set; }

    public long? DeptId { get; set; }

    public List<string> Roles { get; set; } = [];

    public UserListQuery()
    {
    }
}