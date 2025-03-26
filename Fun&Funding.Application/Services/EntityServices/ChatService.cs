using AutoMapper;
using Fun_Funding.Application.ExceptionHandler;
using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.ChatDTO;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;  // Import for IServiceScopeFactory
using MongoDB.Driver;
using System.Net;
using System.Net.WebSockets;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class ChatService : IChatService
    {
        private readonly IWebSocketManager _webSocketManager;
        private readonly IMapper _mapper;
        private readonly IServiceScopeFactory _scopeFactory;  // Inject IServiceScopeFactory

        public ChatService(IWebSocketManager webSocketManager, IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            _webSocketManager = webSocketManager;
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }

        public async Task<ResultDTO<IEnumerable<Chat>>> GetChatConversation(Guid senderId, Guid receiverId)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var filter = Builders<Chat>.Filter.Or(
                        Builders<Chat>.Filter.And(
                            Builders<Chat>.Filter.Eq(m => m.SenderId, senderId),
                            Builders<Chat>.Filter.Eq(m => m.ReceiverId, receiverId)),
                        Builders<Chat>.Filter.And(
                            Builders<Chat>.Filter.Eq(m => m.SenderId, receiverId),
                            Builders<Chat>.Filter.Eq(m => m.ReceiverId, senderId)));

                    var sort = Builders<Chat>.Sort.Descending(m => m.CreatedDate);

                    var chatMessages = await unitOfWork.ChatRepository.GetAllAsync(filter, sort);

                    return ResultDTO<IEnumerable<Chat>>.Success(chatMessages);
                }
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }

                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ResultDTO<IEnumerable<ContactedUserResponse>>> GetContactedUsers(Guid userId, string? name)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var chats = await unitOfWork.ChatRepository.GetAllAsync(
                        Builders<Chat>.Filter.Or(
                            Builders<Chat>.Filter.Eq(chat => chat.SenderId, userId),
                            Builders<Chat>.Filter.Eq(chat => chat.ReceiverId, userId)
                        ));

                    var userIds = chats.Select(chat => chat.SenderId != userId ? chat.SenderId : chat.ReceiverId)
                        .Distinct();

                    var users = unitOfWork.UserRepository
                        .GetQueryable()
                        .Include(user => user.File)
                        .Where(user => userIds.Contains(user.Id));

                    var contactedUsers = new List<ContactedUserResponse>();

                    foreach (var user in users)
                    {
                        var chat = GetChatConversation(userId, user.Id).Result._data.First();

                        var contactedUser = new ContactedUserResponse
                        {
                            UserId = user.Id,
                            LatestMessage = chat.Message,
                            CreatedDate = chat.CreatedDate,
                            Name = !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.UserName,
                            Avatar = user.File != null ? user.File.URL : ""
                        };

                        contactedUsers.Add(contactedUser);
                    }

                    if (!string.IsNullOrEmpty(name))
                    {
                        contactedUsers = contactedUsers.Where(x => x.Name.ToLower().Contains(name.ToLower())).ToList();
                    }

                    var response = contactedUsers.OrderByDescending(x => x.CreatedDate).ToList();

                    return ResultDTO<IEnumerable<ContactedUserResponse>>.Success(response);
                }
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }

                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task HandleWebSocketConnectionAsync(WebSocket webSocket, string senderId, string receiverId)
        {
            try
            {
                await _webSocketManager.HandleConnectionAsync(webSocket, senderId, receiverId);
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }

                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
