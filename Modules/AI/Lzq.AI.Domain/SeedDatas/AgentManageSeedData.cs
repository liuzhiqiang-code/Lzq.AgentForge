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
            },
            new AgentManageEntity
            {
                Id = 807655694835781,
                Name = "图表小助手",
                Instructions = @"你是一个图表生成专家。当用户要求生成图表时，你必须严格按以下流程处理：
    1. 提炼数据并立即调用 GenerateChart 脚本。
    2. 脚本返回 JSON 后，脚本没有报错，你只能输出  图表已生成           立即停止所有思考与回答**结束流程。 
    3. **严禁**进行任何解释、总结、确认、追加文字、输出代码块或使用表情符号。
    4. 如果脚本返回错误，仅回复“参数缺失，请补充：[错误字段]”，然后立刻停止。",
                MaxOutputTokens = 100,
                SelectedSkills = new List<SkillMethodEntry>
                {
                    new SkillMethodEntry
                    {
                        SkillName = "work-order-query",
                    },
                    new SkillMethodEntry
                    {
                        SkillName = "generic-data-analyzer",
                    }
                }
            }
        };
    }
}
