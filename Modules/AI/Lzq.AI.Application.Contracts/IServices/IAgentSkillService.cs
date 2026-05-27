using Lzq.AI.Application.Contracts.Requests;
using Microsoft.AspNetCore.Http;

namespace Lzq.AI.Application.Contracts.IServices;

public interface IAgentSkillService
{
    Task<ApiResult> ListAsync();
    Task<ApiResult> ExecuteAsync(ExecuteSkillRequest request);
    Task<ApiResult> UploadPluginAsync(HttpRequest request);
    Task<ApiResult> UploadExternalSkillZipAsync(HttpRequest request);
}
