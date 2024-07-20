using System.Collections.Generic;
using FitnessApp.NotificationApi.Infrastructure;

namespace FitnessApp.NotificationApi.Factories;

public class WebSocketFactory : IWebSocketFactory
{
    private readonly Dictionary<string, List<WebSocketWrapper>> _webSocketsList = new Dictionary<string, List<WebSocketWrapper>>();

    public void Add(string userId, WebSocketWrapper webSocket)
    {
        if (!_webSocketsList.TryGetValue(userId, out var connectedWebSockets))
            connectedWebSockets = new List<WebSocketWrapper>();

        lock (_webSocketsList)
        {
            connectedWebSockets.Add(webSocket);
            _webSocketsList[userId] = connectedWebSockets;
        }
    }

    public void Remove(string userId, string sessionId)
    {
        if (_webSocketsList.TryGetValue(userId, out var connectedWebSockets))
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
        if (_webSocketsList.TryGetValue(userId, out var connectedWebSockets))
            result = connectedWebSockets.Find(i => i.SessionId == sessionId);
        return result;
    }

    public List<WebSocketWrapper> GetSessionsByClient(string userId)
    {
        if (!_webSocketsList.TryGetValue(userId, out var result))
            result = new List<WebSocketWrapper>();
        return result;
    }
}
