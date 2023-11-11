using System;

namespace FitnessApp.NotificationApi.Infrastructure
{
    public class WebSocketMessage
    {
        public string Type { get; set; }
        public object Data { get; set; }
        public DateTime MessagDateTime { get; set; }
    }
}
