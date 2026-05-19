using Lzq.AI.Domain.Entities;
using Lzq.Extensions.SqlSugar.SeedData;

namespace Lzq.AI.Domain.SeedDatas;

public class AgentManageSeedData : BaseSeedData<AgentManageEntity>
{
    public override List<AgentManageEntity> GetSeedData()
    {
        return new List<AgentManageEntity>
        {
            new AgentManageEntity
            {
                Id = 1,
                Name = "工单小助手",
                MaxOutputTokens = 100,
                SelectedSkills = new List<SkillMethodEntry>
                {
                    new SkillMethodEntry
                    {
                        SkillName = "work-order-demo",
                    },
                }
            }
        };
    }
}
