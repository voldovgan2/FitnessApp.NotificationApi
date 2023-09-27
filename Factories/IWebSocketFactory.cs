using FitnessApp.NotificationApi.Infrastructure;
using System.Collections.Generic;

namespace FitnessApp.NotificationApi.Factories
{
    public interface IWebSocketFactory
    {
        void Add(string userId, WebSocketWrapper webSocket);
        void Remove(string userId, string sessionId);
        WebSocketWrapper GetClient(string userId, string sessionId);
        List<WebSocketWrapper> GetSessionsByClient(string userId);
    }
}
