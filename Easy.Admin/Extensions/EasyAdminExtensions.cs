using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Easy.Admin.Extensions
{
    public static class EasyAdminExtensions
    {
        public static IServiceCollection AddEasyAdmin(this IServiceCollection services)
        {
            services.AddSingleton<Startup>();
            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
               var startup = scope.ServiceProvider.GetRequiredService<Startup>();
                startup.ConfigureServices(services);
            }

            return services;
        }

        public static void UseEasyAdmin(this IApplicationBuilder app)
        {
            var startup = app.ApplicationServices.GetRequiredService<Startup>();
            startup.Configure(app);
        }
    }
}
