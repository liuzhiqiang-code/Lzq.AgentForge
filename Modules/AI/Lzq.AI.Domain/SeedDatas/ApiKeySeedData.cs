using Lzq.AI.Domain.Entities;
using Lzq.Extensions.SqlSugar.SeedData;
using Masa.BuildingBlocks.Data;
using Masa.Utils.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Lzq.AI.Domain.SeedDatas;

public class ApiKeySeedData : BaseSeedData<ApiKeyEntity>
{
    public override List<ApiKeyEntity> GetSeedData()
    {
        var configuration = MasaApp.GetService<IConfiguration>();
        var AesKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            configuration.GetSection("AesKey").Get<string>() ?? ""
            ));
        var keySecret = configuration.GetValue<string>("AIKeySecret:SiliconFlow");
        var ivString = "6baYT3lwQD9XylquKuZyuQ==";

        // 使用明确的重载：Encrypt(内容, 密钥, IV字节数组)
        var keyValue = AesUtils.Encrypt(keySecret, AesKey, ivString);


        return new List<ApiKeyEntity>
        {
            new ApiKeyEntity
            {
                Id = 1,
                Provider = Enums.ProviderEnum.SiliconFlow,
                KeyName = "SiliconFlow.Pro",
                KeyValue =keyValue,
                KeyIv = ivString,
                BaseUrl = "https://api.siliconflow.cn/v1",
                IsEnabled = true,
            }
        };
    }
}
