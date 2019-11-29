using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Easy.Admin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        //.UseKestrel(serverOptions =>
                        //{
                        //    // Set properties and call methods on options
                        //})
                        //.UseIISIntegration()
                        .UseStartup<Startup>();
                });
    }
}
