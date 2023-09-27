using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using FitnessApp.NotificationApi.Factories;

namespace FitnessApp.NotificationApi.Infrastructure
{
    public class WebSocketManager
    {
        public WebSocketManager(RequestDelegate next) 
        {

        }

        public async Task Invoke(HttpContext context, IWebSocketFactory factory)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var authenticationResult = await context.AuthenticateAsync();
                    if (authenticationResult.Succeeded)
                    {
                        var userId = (context.User.Identity as ClaimsIdentity).Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                        if (!string.IsNullOrEmpty(userId))
                        {
                            string sessionId = context.Request.Query["sessionId"];
                            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(sessionId))
                            {
                                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                                WebSocketWrapper userWebSocket = new WebSocketWrapper()
                                {
                                    Socket = webSocket,
                                    SessionId = sessionId
                                };
                                factory.Add(userId, userWebSocket);
                                var buffer = new byte[1024 * 4];
                                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                                while (!result.CloseStatus.HasValue)
                                {
                                    buffer = new byte[1024 * 4];
                                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                                }
                                factory.Remove(userId, userWebSocket.SessionId);
                                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                            }
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 401;
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
        }
    }
}