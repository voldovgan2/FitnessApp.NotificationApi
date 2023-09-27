using FitnessApp.NotificationApi.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace FitnessApp.NotificationApi.Factories
{
    public class WebSocketFactory : IWebSocketFactory
    {
        private Dictionary<string, List<WebSocketWrapper>> _webSocketsList = new Dictionary<string, List<WebSocketWrapper>>();
                
        public void Add(string userId, WebSocketWrapper webSocket)
        {
            List<WebSocketWrapper> connectedWebSockets;
            if (!_webSocketsList.TryGetValue(userId, out connectedWebSockets))
            {
                connectedWebSockets = new List<WebSocketWrapper>();
            }
            lock (_webSocketsList)
            {   
                connectedWebSockets.Add(webSocket);
                _webSocketsList[userId] = connectedWebSockets;
            }
        }

        public void Remove(string userId, string sessionId)
        {
            List<WebSocketWrapper> connectedWebSockets;
            if (_webSocketsList.TryGetValue(userId, out connectedWebSockets))
            {
                lock (_webSocketsList)
                {
                    connectedWebSockets.RemoveAll(i => i.SessionId == sessionId);
                    if (connectedWebSockets.Count == 0)
                    {
                        _webSocketsList.Remove(userId);
                    }
                }
            }
        }

        public WebSocketWrapper GetClient(string userId, string sessionId)
        {
            WebSocketWrapper result = null;
            List<WebSocketWrapper> connectedWebSockets;
            if (_webSocketsList.TryGetValue(userId, out connectedWebSockets))
            {   
                result = connectedWebSockets.FirstOrDefault(i => i.SessionId == sessionId);
            }
            return result;
        }

        public List<WebSocketWrapper> GetSessionsByClient(string userId)
        {
            List<WebSocketWrapper> result;
            if (!_webSocketsList.TryGetValue(userId, out result))
            {
                result = new List<WebSocketWrapper>();
            }
            return result;
        }
    }
}
