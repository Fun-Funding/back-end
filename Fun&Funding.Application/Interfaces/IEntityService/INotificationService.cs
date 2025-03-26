using Fun_Funding.Domain.Entity.NoSqlEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Interfaces.IEntityService
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetUserNotifications(Guid userId);
        Task SendNotification(Notification notification, List<Guid> userIds);
        Task MarkAsRead(Guid notificationId, Guid userId);
    }
}
