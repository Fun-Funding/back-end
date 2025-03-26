using System.Net.WebSockets;

namespace Fun_Funding.Application.Interfaces.IExternalServices
{
    public interface IWebSocketManager
    {
        Task HandleConnectionAsync(WebSocket webSocket, string senderId, string receiverId);
        Task BroadcastMessage(string message, string senderId, string receiverId);
    }
}
