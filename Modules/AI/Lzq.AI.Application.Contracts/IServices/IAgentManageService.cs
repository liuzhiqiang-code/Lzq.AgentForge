using Lzq.AI.Application.Contracts.Commands;
using Lzq.AI.Application.Contracts.Queries;

namespace Lzq.AI.Application.Contracts.IServices;

public interface IAgentManageService
{
    Task<ApiResult> PageAsync(AgentManagePageQuery query);
    Task<ApiResult> ListAsync();
    Task<ApiResult> CreateAsync(AgentManageCreateCommand command);
    Task<ApiResult> UpdateAsync(AgentManageUpdateCommand command);
    Task<ApiResult> DeleteAsync(long id);
    Task<ApiResult> BatchDeleteAsync(List<long> ids);
    Task<AIAgentModel> GetAIAgentModel(string aiAgentName);
}
