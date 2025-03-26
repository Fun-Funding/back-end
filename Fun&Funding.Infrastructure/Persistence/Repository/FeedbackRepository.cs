using Fun_Funding.Application.Interfaces.IRepository;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Fun_Funding.Infrastructure.Persistence.Database;


namespace Fun_Funding.Infrastructure.Persistence.Repository
{
    public class FeedbackRepository : MongoBaseRepository<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(MongoDBContext mongoDB) : base(mongoDB, "feedback")
        {
        }
    }
}
