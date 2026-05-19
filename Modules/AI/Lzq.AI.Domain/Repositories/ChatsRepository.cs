using Lzq.Extensions.SqlSugar.Repository;
using Lzq.AI.Domain.IRepositories;
using Lzq.AI.Domain.Entities;

namespace Lzq.AI.Domain.Repositories;

public class ChatsRepository()
    : SqlSugarRepository<ChatsEntity>(), IChatsRepository
{

}