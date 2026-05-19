using Lzq.AI.Application.Contracts.Commands;
using Lzq.AI.Application.Contracts.Requests;
using Microsoft.AspNetCore.Http;

namespace Lzq.AI.Application.Contracts.IServices;

public interface IAgentSkillService
{
    Task<ApiResult> ListAsync();
    Task<ApiResult> CreateAsync(AgentManageCreateCommand command);
    Task<ApiResult> UpdateAsync(AgentManageUpdateCommand command);
    Task<ApiResult> DeleteAsync(long id);
    Task<ApiResult> BatchDeleteAsync(List<long> ids);
    Task<ApiResult> ExecuteAsync(ExecuteSkillRequest request);
    Task<ApiResult> UploadPluginAsync(HttpRequest request);
    Task<ApiResult> UploadExternalSkillZipAsync(HttpRequest request);
}
