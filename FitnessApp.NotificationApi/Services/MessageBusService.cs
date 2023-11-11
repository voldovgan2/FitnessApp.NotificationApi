using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using FitnessApp.Common.Serializer.JsonSerializer;
using FitnessApp.Common.ServiceBus.Nats;
using FitnessApp.Common.ServiceBus.Nats.Events;
using FitnessApp.Common.ServiceBus.Nats.Services;
using FitnessApp.NotificationApi.Contracts;
using FitnessApp.NotificationApi.Factories;
using FitnessApp.NotificationApi.Infrastructure;
using Microsoft.Extensions.Hosting;
using NATS.Client;

namespace FitnessApp.Services.NotificationApi
{
    public class MessageBusService : IHostedService
    {
        private readonly IJsonSerializer _serializer;
        private readonly IServiceBus _serviceBus;
        private readonly IWebSocketFactory _webSocketFactory;
        private IAsyncSubscription _eventSubscription;

        public MessageBusService(
            IServiceBus serviceBus,
            IWebSocketFactory webSocketFactory,
            IJsonSerializer serializer)
        {
            _serviceBus = serviceBus;
            _webSocketFactory = webSocketFactory;
            _serializer = serializer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _eventSubscription = _serviceBus.SubscribeEvent(Topic.FOLLOW_REQUEST_CONFIRMED, async (sender, args) =>
            {
                var integrationEvent = _serializer.DeserializeFromBytes<FollowRequestConfirmed>(args.Message.Data);
                var sessions = _webSocketFactory.GetSessionsByClient(integrationEvent.UserId);
                foreach (var session in sessions)
                {
                    var webSocketMessage = new WebSocketMessage
                    {
                        Type = ContactConfirmedContract.CONTRACT_TYPE,
                        Data = new ContactConfirmedContract
                        {
                            UserId = integrationEvent.UserId,
                            FollowerUserId = integrationEvent.FollowerUserId
                        },
                        MessagDateTime = DateTime.UtcNow
                    };
                    byte[] bytes = _serializer.SerializeToBytes(webSocketMessage);
                    await session.Socket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _eventSubscription.Unsubscribe();
            return Task.CompletedTask;
        }
    }
}