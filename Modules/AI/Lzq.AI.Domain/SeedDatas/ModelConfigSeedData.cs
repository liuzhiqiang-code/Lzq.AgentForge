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
                Id = 1,
                ApiKeyId = 1,
                ConfigName = "deepseek-ai/DeepSeek-V4-Flash",
                DisplayModelName = "deepseek-ai/DeepSeek-V4-Flash",
                IsEnabled = true
            }
        };
    }
}
