using Fun_Funding.Application.AppHub;
using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.ViewModel.NotificationDTO;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationController(INotificationService notificationService, IHubContext<NotificationHub> hubContext)
        {
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserNotifications(Guid userId)
        {
            var notifications = await _notificationService.GetUserNotifications(userId);
            return Ok(notifications);
        }

        //[HttpPost("send")]
        //public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
        //{
        //    if (request == null || request.Notification == null || request.UserIds == null)
        //    {
        //        return BadRequest("Invalid request.");
        //    }

        //    await _notificationService.SendNotification(request.Notification, request.UserIds);
        //    return Ok();
        //}

    }
}
