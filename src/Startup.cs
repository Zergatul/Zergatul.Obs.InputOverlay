using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
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
            string[] addresses = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses.ToArray();

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
                    if (addresses.Any(a1 => context.Request.Headers.Origin.Any(a2 => OriginMatch(a1, a2))))
                    {
                        using (var ws = await context.WebSockets.AcceptWebSocketAsync())
                        {
                            await handler.HandleWebSocket(ws);
                        }
                    }
                    else
                    {
                        // deny requests from other origins
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    }
                }
                else
                {
                    await next();
                }
            });
        }

        private bool OriginMatch(string origin1, string origin2)
        {
            Uri uri1 = new Uri(origin1);
            Uri uri2 = new Uri(origin2);
            return uri1.Scheme == uri2.Scheme && string.Equals(uri1.Host, uri2.Host, StringComparison.OrdinalIgnoreCase) && uri1.Port == uri2.Port;
        }
    }
}