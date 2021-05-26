using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace Zergatul.Obs.InputOverlay
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(builder =>
            {
                builder.UseStartup<Startup>();
                builder.UseWebRoot("wwwroot");
                builder.UseUrls("http://localhost:5001/");
                builder.UseKestrel();
            });
        }
    }
}