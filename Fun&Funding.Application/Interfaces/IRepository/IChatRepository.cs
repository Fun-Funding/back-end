using Fun_Funding.Domain.Entity.NoSqlEntities;

namespace Fun_Funding.Application.IRepository
{
    public interface IChatRepository : IMongoBaseRepository<Chat>
    {
    }
}
