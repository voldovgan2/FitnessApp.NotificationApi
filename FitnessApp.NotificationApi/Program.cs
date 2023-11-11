using System;
using System.Threading.Tasks;
using FitnessApp.Common.Serializer.JsonSerializer;
using FitnessApp.Common.ServiceBus.Nats.Services;
using FitnessApp.NotificationApi.Factories;
using FitnessApp.NotificationApi.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IServiceBus, ServiceBus>();

builder.Services.AddHostedService<FitnessApp.Services.NotificationApi.MessageBusService>();

builder.Services.AddSingleton<IWebSocketFactory, WebSocketFactory>();

builder.Services.AddTransient<IJsonSerializer, JsonSerializer>();

builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(cfg =>
    {
        cfg.RequireHttpsMetadata = false;
        cfg.Authority = builder.Configuration["JWT:Issuer"];
        cfg.Audience = builder.Configuration["JWT:Audience"];
        cfg.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Query["access_token"];
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();

var webSocketOptions = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromSeconds(120),
};
app.UseWebSockets(webSocketOptions);
app.UseMiddleware<WebSocketManager>();

app.Run();

public partial class Program { }