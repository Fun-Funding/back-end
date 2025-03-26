using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.ChatDTO;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using System.Net.WebSockets;

namespace Fun_Funding.Application.IService
{
    public interface IChatService
    {
        Task HandleWebSocketConnectionAsync(WebSocket webSocket, string senderId, string receiverId);
        Task<ResultDTO<IEnumerable<Chat>>> GetChatConversation(Guid senderId, Guid receiverId);
        Task<ResultDTO<IEnumerable<ContactedUserResponse>>> GetContactedUsers(Guid userId, string? name);
    }
}
