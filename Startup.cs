using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Zergatul.Obs.InputOverlay
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IWebSocketHandler, WebSocketHandler>();
            services.AddSingleton<IInputHook, InputHook>();

            //services.AddSingleton<IInputHook, EmptyInputHook>();
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