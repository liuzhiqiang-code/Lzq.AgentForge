using Lzq.Extensions.SqlSugar.SeedData;
using Lzq.Rbac.Domain.Entities;

namespace Lzq.Rbac.Domain.SeedDatas;

public class RoleSeedData : BaseSeedData<RoleEntity>
{
    public override List<RoleEntity> GetSeedData()
    {
        return new List<RoleEntity>
        {
            new RoleEntity
            {
                Id = 1,
                Name = "管理员",
                Status = Enums.EnableStatusEnum.Enabled,
                Remark = ""
            },
            new RoleEntity
            {
                Id = 2,
                Name = "普通用户",
                Status = Enums.EnableStatusEnum.Enabled,
                Remark = ""
            },
        };
    }
}
