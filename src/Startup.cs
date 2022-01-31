using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zergatul.Obs.InputOverlay.RawInput;
using Zergatul.Obs.InputOverlay.RawInput.Device;
using Zergatul.Obs.InputOverlay.XInput;

namespace Zergatul.Obs.InputOverlay
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IWebSocketHandler, WebSocketHandler>();
            services.AddSingleton<IRawDeviceInput, RawDeviceInput>();
            services.AddSingleton<IRawDeviceFactory, RawDeviceFactory>();
            services.AddSingleton<IXInputHandler, XInputHandler>();

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
            });
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime hostAppLifetime, IWebSocketHandler handler)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();

            hostAppLifetime.ApplicationStopping.Register(() =>
            {
                handler.Dispose();
            });

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
                {
                    using (var ws = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        await handler.HandleWebSocket(ws);
                    }
                }
                else
                {
                    await next();
                }
            });
        }
    }
}