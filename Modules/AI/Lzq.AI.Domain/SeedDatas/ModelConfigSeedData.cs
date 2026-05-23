using Lzq.AI.Domain.Entities;
using Lzq.Extensions.SqlSugar.SeedData;

namespace Lzq.AI.Domain.SeedDatas;

public class ModelConfigSeedData : BaseSeedData<ModelConfigEntity>
{
    public override List<ModelConfigEntity> GetSeedData()
    {
        return new List<ModelConfigEntity>
        {
            new ModelConfigEntity
            {
                Id = 807284909445189,
                ApiKeyId = 1,
                ConfigName = "MiniMaxM25",
                DisplayModelName = "MiniMaxAI/MiniMax-M2.5",
                IsEnabled = true
            }
        };
    }
}
