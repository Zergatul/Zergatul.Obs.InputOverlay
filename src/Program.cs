using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Zergatul.Obs.InputOverlay
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            return ApplicationOptionsParser.ParseAndRun(args, Run);
        }
        
        private static int Run(ApplicationOptions options)
        {
            try
            {
                CreateWebHostBuilder(options).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                HandleStartupException(ex, options.EnableDebugLog);
                return 1;
            }
        }

        private static IHostBuilder CreateWebHostBuilder(ApplicationOptions options)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((_, configuration) =>
                {
                    if (options.EnableDebugLog)
                    {
                        configuration.AddInMemoryCollection(new Dictionary<string, string>
                        {
                            ["Logging:LogLevel:Zergatul"] = "Debug"
                        });
                    }
                })
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(options);
                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseStartup<Startup>();
                    builder.UseWebRoot(Path.Combine(AppContext.BaseDirectory, "wwwroot"));
                    builder.UseUrls($"http://{options.Host}:{options.Port}/");
                    builder.UseKestrel();
                });
        }

        private static void HandleStartupException(Exception ex, bool showStackTrace)
        {
            PrintStartupExceptionCause(ex);

            if (showStackTrace)
            {
                Console.WriteLine(ex);
            }
        }

        private static void PrintStartupExceptionCause(Exception ex)
        {
            if (ex is SocketException socketException)
            {
                if (socketException.SocketErrorCode == SocketError.AddressNotAvailable)
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("Specified listen ip/host is not valid.");
                    Console.Error.WriteLine($"Verify {ApplicationOptionsParser.ListenOptionName} is correct.");
                    Console.Error.WriteLine();
                }

                return;
            }

            if (ex.InnerException != null)
            {
                PrintStartupExceptionCause(ex.InnerException);
            }
        }
    }
}