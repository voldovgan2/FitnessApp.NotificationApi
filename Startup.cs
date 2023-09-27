using System;
using System.Threading.Tasks;
using FitnessApp.NatsServiceBus;
using FitnessApp.NotificationApi.Factories;
using FitnessApp.NotificationApi.Infrastructure;
using FitnessApp.Serializer.JsonMapper;
using FitnessApp.Serializer.JsonSerializer;
using FitnessApp.Services.NotificationApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FitnessApp.NotificationApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            });

            services.AddSingleton<IServiceBus, ServiceBus>();

            services.AddHostedService<MessageBusService>();

            services.AddSingleton<IWebSocketFactory, WebSocketFactory>();

            services.AddTransient<IJsonSerializer, JsonSerializer>();

            services.AddTransient<IJsonMapper, JsonMapper>();
                        
            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.Authority = Configuration["JWT:Issuer"];
                    cfg.Audience = Configuration["JWT:Audience"]; 
                    cfg.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Query["access_token"];
                            return Task.CompletedTask;
                        }
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            loggerFactory.AddFile("Logs/NotificationApi-{Date}.txt");

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseMvc();

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(webSocketOptions);
            app.UseMiddleware<WebSocketManager>();
        }
    }
}