using System.Net.WebSockets;

namespace FitnessApp.NotificationApi.Infrastructure
{
    public class WebSocketWrapper
    {
        public string SessionId { get; set; }
        public WebSocket Socket { get; set; }
    }
}