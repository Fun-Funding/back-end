using Fun_Funding.Application.IRepository;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Fun_Funding.Infrastructure.Persistence.Database;

namespace Fun_Funding.Infrastructure.Persistence.Repository
{
    public class ChatRepository : MongoBaseRepository<Chat>, IChatRepository
    {
        public ChatRepository(MongoDBContext mongoDB) : base(mongoDB, "chat")
        {
        }
    }
}
