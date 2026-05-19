using Lzq.AI.Domain.Entities;
using Lzq.AI.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;

namespace Lzq.AI.Domain.Repositories;

public class AgentManageRepository()
    : SqlSugarRepository<AgentManageEntity>(), IAgentManageRepository
{

}