using Fun_Funding.Application.IRepository;
using Fun_Funding.Domain.Entity.NoSqlEntities;

namespace Fun_Funding.Application.Interfaces.IRepository
{
    public interface IFeedbackRepository : IMongoBaseRepository<Feedback>
    {
    }
}
