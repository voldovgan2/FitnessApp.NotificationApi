using FitnessApp.IntegrationEvents;
using FitnessApp.NatsServiceBus;
using FitnessApp.NotificationApi.Factories;
using FitnessApp.NotificationApi.Infrastructure;
using FitnessApp.NotificationApi.Models;
using FitnessApp.Serializer.JsonMapper;
using FitnessApp.Serializer.JsonSerializer;
using Microsoft.Extensions.Hosting;
using NATS.Client;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace FitnessApp.Services.NotificationApi
{
    public class MessageBusService : IHostedService
    {
        private readonly IServiceBus _serviceBus;
        private readonly IWebSocketFactory _webSocketFactory;
        private IAsyncSubscription _eventSubscription;
        private readonly IJsonSerializer _serializer;
        private readonly IJsonMapper _mapper;

        public MessageBusService(IServiceBus serviceBus, IWebSocketFactory webSocketFactory, IJsonSerializer serializer, IJsonMapper mapper)
        {
            _serviceBus = serviceBus;
            _webSocketFactory = webSocketFactory;
            _serializer = serializer;
            _mapper = mapper;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _eventSubscription = _serviceBus.SubscribeEvent(Topic.FOLLOW_REQUEST_CONFIRMED, (sender, args) => 
            {
                var integrationEvent = _serializer.DeserializeFromBytes<FollowRequestConfirmedEvent>(args.Message.Data);
                var sessions = _webSocketFactory.GetSessionsByClient(integrationEvent.UserId);
                foreach (var session in sessions)
                {
                    var webSocketMessage = new WebSocketMessage
                    {
                        Type = "ContactConfirmationEvent",
                        Data = _mapper.Convert<ContactConfirmedModel>(integrationEvent),
                        MessagDateTime = DateTime.UtcNow
                    };
                    byte[] bytes = _serializer.SerializeToBytes(webSocketMessage);
                    session.Socket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
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