using Fun_Funding.Application.Interfaces.IRepository;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Fun_Funding.Infrastructure.Persistence.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Infrastructure.Persistence.Repository
{
    public class NotificationRepository : MongoBaseRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(MongoDBContext mongoDB) : base(mongoDB, "notification")
        {
        }
    }
}
