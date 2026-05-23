using Lzq.AI.Application.Contracts.Commands;
using Lzq.AI.Application.Contracts.Dtos;
using Lzq.AI.Application.Contracts.IServices;
using Lzq.AI.Application.Contracts.Queries;
using Lzq.AI.Application.Contracts.Requests;
using Lzq.AI.Application.Contracts.Responses;
using Lzq.AI.Domain.Entities;
using Lzq.AI.Domain.Enums;
using Lzq.AI.Domain.IRepositories;
using Lzq.Core.Models;
using Lzq.Extensions.AI;
using Lzq.Extensions.AI.Consts;
using Masa.Utils.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSwag.Annotations;
using SqlSugar;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Lzq.AI.Application.Services;

public class ApiKeyService : ServiceBase, IApiKeyService
{
    public ApiKeyService() : base("/api/v1/ai/apiKey") { }

    private string AesKey => Convert.ToBase64String(Encoding.UTF8.GetBytes(
            Configuration.GetSection("AesKey").Get<string>() ?? ""
            ));
    private IConfiguration Configuration => GetRequiredService<IConfiguration>();
    private IHttpClientFactory HttpClientFactory => GetRequiredService<IHttpClientFactory>();
    private IApiKeyRepository ApiKeyRepository => GetRequiredService<IApiKeyRepository>();
    private IModelConfigRepository ModelConfigRepository => GetRequiredService<IModelConfigRepository>();

    [OpenApiTag("ai/apiKey"), OpenApiOperation("获取分页列表", "")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult> PageAsync([FromBody] ApiKeyPageQuery query)
    {
        RefAsync<int> total = 0;
        var pageList = await ApiKeyRepository.AsQueryable().ToPageListAsync(query.Page, query.PageSize, total);
        foreach (var item in pageList)
        {
            string plainKey = AesUtils.Decrypt(item.KeyValue, AesKey, item.KeyIv);
            item.KeyValue = MaskApiKey(plainKey);
        }
        var result = pageList.Map<List<ApiKeyViewDto>>();
        return ApiResult.Success(new PagedResponse<ApiKeyViewDto>(result, total));
    }

    [OpenApiTag("ai/apiKey"), OpenApiOperation("获取列表", "")]
    [RoutePattern(pattern: "list", true)]
    public async Task<ApiResult> ListAsync()
    {
        var list = await ApiKeyRepository.AsQueryable()
            .ToListAsync();
        foreach (var item in list)
        {
            string plainKey = AesUtils.Decrypt(item.KeyValue, AesKey, item.KeyIv);
            item.KeyValue = MaskApiKey(plainKey);
        }
        return ApiResult.Success(list.Map<List<ApiKeyViewDto>>());
    }

    [OpenApiTag("ai/apiKey"), OpenApiOperation("明细", "")]
    [RoutePattern(pattern: "detail/{id}", true, HttpMethod = "GET")]
    public async Task<ApiResult> DetailAsync(long id)
    {
        var entity = await ApiKeyRepository.GetByIdAsync(id);
        string plainKey = AesUtils.Decrypt(entity.KeyValue, AesKey, entity.KeyIv);
        entity.KeyValue = MaskApiKey(plainKey);
        var result = entity.Map<ApiKeyViewDto>();

        result.SelectModel = await ModelConfigRepository.AsQueryable()
            .Where(m => m.ApiKeyId == id && m.IsEnabled)
            .Select(m => m.DisplayModelName)
            .ToListAsync();
        var modelsResult = await GetAvailableModelsAsync(new GetAvailableModelsRequest(entity.Provider, plainKey));
        if (!modelsResult.IsSuccess)
            return ApiResult.Fail(modelsResult.Message);
        result.ShowModel = modelsResult.Data!;
        return ApiResult.Success(result);
    }

    [OpenApiTag("ai/apiKey"), OpenApiOperation("根据厂商和Key获取可用模型", "")]
    [RoutePattern(pattern: "getAvailableModels", true, HttpMethod = "POST")]
    public async Task<ApiResult<List<string>>> GetAvailableModelsAsync([FromBody] GetAvailableModelsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.KeyValue) || request.Provider == 0)
            return ApiResult.Success(new List<string>());

        if (request.Provider == ProviderEnum.DeepSeek)
        {
            try
            {
                var client = HttpClientFactory.CreateClient("DeepSeekClient");
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.deepseek.com/models");
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.KeyValue);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var modelList = JsonSerializer.Deserialize<DeepSeekModelListResponse>(content);

                var models = modelList?.Data.Select(m => m.Id).ToList() ?? new List<string>();
                return ApiResult.Success(models);
            }
            catch (Exception)
            {
                throw new UserFriendlyException("ApiKey无效！");
            }
        }
        else if (request.Provider == ProviderEnum.SiliconFlow)
        {
            try
            {
                var client = HttpClientFactory.CreateClient("SiliconFlowClient");
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.siliconflow.cn/v1/models");
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.KeyValue);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var modelList = JsonSerializer.Deserialize<SiliconFlowModelListResponse>(content);

                var models = modelList?.Data.Select(m => m.Id).ToList() ?? new List<string>();
                return ApiResult.Success(models);
            }
            catch (Exception)
            {
                throw new UserFriendlyException("ApiKey无效！");
            }
        }

        return ApiResult.Success(new List<string>());
    }

    [OpenApiTag("ai/apiKey"), OpenApiOperation("增加", "")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult> CreateAsync([FromBody] ApiKeyCreateCommand command)
    {
        var entity = command.Map<ApiKeyEntity>();
        entity.BaseUrl = entity.Provider switch
        {
            ProviderEnum.DeepSeek => "https://api.deepseek.com/v1",
            ProviderEnum.SiliconFlow => "https://api.siliconflow.cn/v1",
            _ => ""
        };
        EncryptApiKey(entity);

        // 调用接口获取支持的模型
        string plainKey = AesUtils.Decrypt(entity.KeyValue, AesKey, entity.KeyIv);
        var modelsResult = await GetAvailableModelsAsync(new GetAvailableModelsRequest(entity.Provider, plainKey));
        if (!modelsResult.IsSuccess)
            return ApiResult.Fail(modelsResult.Message);
        var modelConfigs = modelsResult.Data!.Select(modelId => new ModelConfigEntity
        {
            ApiKeyId = entity.Id,
            ConfigName = modelId,
            DisplayModelName = modelId,
            IsEnabled = false
        }).ToList();
        foreach (var item in modelConfigs)
        {
            if (command.SelectModel.Contains(item.DisplayModelName))
            {
                item.IsEnabled = true;
            }
        }
        await ApiKeyRepository.InsertAsync(entity);
        await ModelConfigRepository.InsertRangeAsync(modelConfigs);
        return ApiResult.Success();
    }

    [OpenApiTag("ai/apiKey"), OpenApiOperation("更新", "")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult> UpdateAsync([FromBody] ApiKeyUpdateCommand command)
    {
        var entity = await ApiKeyRepository.GetByIdAsync(command.Id);
        entity.KeyName = command.KeyName;
        entity.IsEnabled = command.IsEnabled;

        // 调用接口获取支持的模型
        var modelConfigs = await ModelConfigRepository.AsQueryable()
            .Where(m => m.ApiKeyId == command.Id)
            .ToListAsync();
        foreach (var item in modelConfigs)
        {
            item.IsEnabled = false;
            if (command.SelectModel.Contains(item.DisplayModelName))
            {
                item.IsEnabled = true;
            }
        }
        await ApiKeyRepository.UpdateAsync(entity);
        await ModelConfigRepository.UpdateRangeAsync(modelConfigs);
        return ApiResult.Success();
    }

    [OpenApiTag("ai/apiKey"), OpenApiOperation("删除", "")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult> DeleteAsync(long id)
    {
        await ApiKeyRepository.DeleteAsync(a => id.Equals(a.Id));
        await ModelConfigRepository.DeleteAsync(a => a.ApiKeyId == id);
        return ApiResult.Success();
    }

    [OpenApiTag("ai/apiKey"), OpenApiOperation("批量删除", "")]
    [RoutePattern(pattern: "batchDelete", true, HttpMethod = "Delete")]
    public async Task<ApiResult> BatchDeleteAsync([FromBody] List<long> ids)
    {
        await ApiKeyRepository.DeleteAsync(a => ids.Contains(a.Id));
        await ModelConfigRepository.DeleteAsync(a => ids.Contains(a.ApiKeyId));
        return ApiResult.Success();
    }

    /// <summary>
    /// 加密 API Key
    /// </summary>
    /// <param name="entity"></param>
    private void EncryptApiKey(ApiKeyEntity entity)
    {
        // 随机生成 16 字节 IV，转为 Base64 字符串（Masa 内部会处理字节转换）
        byte[] ivBytes = new byte[16];
        System.Security.Cryptography.RandomNumberGenerator.Fill(ivBytes);
        string ivString = Convert.ToBase64String(ivBytes);

        // 使用明确的重载：Encrypt(内容, 密钥, IV字节数组)
        entity.KeyValue = AesUtils.Encrypt(entity.KeyValue, AesKey, ivString);
        entity.KeyIv = ivString;
    }

    /// <summary>
    /// 对 API Key 进行脱敏处理，仅保留前后各4位字符，中间用星号替代。
    /// </summary>
    /// <param name="apiKey">明文的 API Key</param>
    /// <returns>脱敏后的字符串，如：sk-1***z9</returns>
    private string MaskApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length <= 8)
            return apiKey; // 太短的 key 直接返回

        const int keepLength = 4;
        return $"{apiKey[..keepLength]}{new string('*', apiKey.Length - keepLength * 2)}{apiKey[^keepLength..]}";
    }

    public async Task<AISetting> GetAISettingAsync(long aIModelConfigId)
    {
        var data = await ModelConfigRepository.AsQueryable()
            .LeftJoin<ApiKeyEntity>((a, b) => a.ApiKeyId == b.Id)
            .Where(a => a.Id.Equals(aIModelConfigId))
            .Select((a, b) => new
            {
                b.KeyName,
                a.ConfigName,
                a.Id,
                b.BaseUrl,
                b.KeyValue,
                b.KeyIv,
                a.DisplayModelName
            }).FirstAsync();
        AISetting setting;
        if (data != null)
        {
            setting = new AISetting
            {
                ConfigId = data.KeyName + "_" + data.ConfigName + "_" + data.Id,
                Url = data.BaseUrl,
                Model = data.DisplayModelName,
                KeySecret = AesUtils.Decrypt(data.KeyValue, AesKey, data.KeyIv)
            };
        }
        else
        {
            var keySecret = Configuration.GetSection("AIKeySecret:SiliconFlow").Get<string>() ?? "";
            setting = ChatClientConst.DeepSeek_V4_Flash;
            setting.KeySecret = keySecret;
        }
        return setting;
    }
}
