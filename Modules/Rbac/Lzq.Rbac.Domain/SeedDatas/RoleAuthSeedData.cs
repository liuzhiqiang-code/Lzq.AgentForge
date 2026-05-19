using Lzq.Extensions.SqlSugar.SeedData;
using Lzq.Rbac.Domain.Entities;

namespace Lzq.Rbac.Domain.SeedDatas;

public class RoleAuthSeedData : BaseSeedData<RoleAuthEntity>
{
    public override List<RoleAuthEntity> GetSeedData()
    {
        return new List<RoleAuthEntity>
        {
            new RoleAuthEntity
            {
                Id = 1,
                RoleId = 2,
                MenuId = 2,
            },
            new RoleAuthEntity
            {
                Id = 2,
                RoleId = 2,
                MenuId = 201,
            },
            new RoleAuthEntity
            {
                Id = 3,
                RoleId = 2,
                MenuId = 202,
            },
            new RoleAuthEntity
            {
                Id = 4,
                RoleId = 2,
                MenuId = 203,
            },
            new RoleAuthEntity
            {
                Id = 5,
                RoleId = 2,
                MenuId = 204,
            },
            new RoleAuthEntity
            {
                Id = 6,
                RoleId = 2,
                MenuId = 205,
            },
        };
    }
}
