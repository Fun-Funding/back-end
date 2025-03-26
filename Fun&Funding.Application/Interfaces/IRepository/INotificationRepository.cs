using Fun_Funding.Application.IRepository;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Interfaces.IRepository
{
    public interface INotificationRepository : IMongoBaseRepository<Notification>
    {
    }
}
