using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FitnessApp.NotificationApi.Factories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace FitnessApp.NotificationApi.Infrastructure
{
    public class WebSocketManager(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context, IWebSocketFactory factory)
        {
            if (context.Request.Path != "/ws")
            {
                await next(context);
                return;
            }

            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            var authenticationResult = await context.AuthenticateAsync();
            if (!authenticationResult.Succeeded)
            {
                context.Response.StatusCode = 401;
                return;
            }

            var userId = (context.User.Identity as ClaimsIdentity).Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return;

            var sessionId = context.Request.Query["sessionId"];
            if (string.IsNullOrWhiteSpace(sessionId))
                return;

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var userWebSocket = new WebSocketWrapper
            {
                Socket = webSocket,
                SessionId = sessionId
            };
            factory.Add(userId, userWebSocket);
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
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