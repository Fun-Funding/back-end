using Fun_Funding.Application.AppHub;
using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IUnitOfWork unitOfWork, IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }
        public async Task<IEnumerable<Notification>> GetUserNotifications(Guid userId)
        {
            try
            {
                var filter = Builders<Notification>.Filter.Exists(n => n.UserReadStatus[userId.ToString()], true);
                var sort = Builders<Notification>.Sort.Descending(n => n.Date);

                var notifications = await _unitOfWork.NotificationRepository.GetAllAsync(filter, sort);

                return notifications;
            }
            catch (Exception e)
            {
                throw new Exception($"Error retrieving notifications: {e.Message}");
            }
        }

        public async Task MarkAsRead(Guid notificationId, Guid userId)
        {
            try
            {
                var notification = _unitOfWork.NotificationRepository.Get(n => n.Id == notificationId);

                if (notification == null)
                {
                    throw new Exception("Notification not found!");
                }

                if (notification.UserReadStatus.TryGetValue(userId.ToString(), out bool isRead) && isRead)
                {
                    return;
                }

                var update = Builders<Notification>.Update.Set(n => n.UserReadStatus[userId.ToString()], true);
                _unitOfWork.NotificationRepository.Update(x => x.Id == notificationId, update);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to mark notification as read: {e.Message}");
            }
        }

        public async Task SendNotification(Notification notification, List<Guid> userIds)
        {
            try
            {
                notification.Id = Guid.NewGuid();
                notification.Date = DateTime.UtcNow;
                notification.ObjectId = notification.ObjectId;
                notification.UserReadStatus = userIds.ToDictionary(userId => userId.ToString(), _ => false);

                await _unitOfWork.NotificationRepository.CreateAsync(notification);
                await _hubContext.Clients.Users(userIds.Select(id => id.ToString())).SendAsync("ReceiveNotification", notification);
                //await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
            }
            catch (Exception e)
            {
                throw new Exception($"Error sending notification: {e.Message}");
            }
        }
    }
}
