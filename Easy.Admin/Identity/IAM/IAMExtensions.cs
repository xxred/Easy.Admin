using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Easy.Admin.Identity.IAM.Endpoints;
using Easy.Admin.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using XCode.Membership;

namespace Easy.Admin.Identity.IAM
{
    public static class IAMExtensions
    {
        /// <summary>
        /// 添加IAM服务
        /// </summary>
        /// <param name="services"></param>
        public static void AddIAMService(this IServiceCollection services)
        //where TUser : User<TUser>, new()
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var options = new IAMOptions();

            configuration.Bind("IAM",options);
            services.TryAddSingleton(options);

            services.AddSingleton<EndpointBase, ChangePasswordEndpoint>();
            services.AddSingleton<EndpointBase, GetUserInfoEndpoint>();
            services.AddSingleton<EndpointBase, LoginEndpoint>();
            services.AddSingleton<EndpointBase, RegisterEndpoint>();
            services.AddSingleton<EndpointBase, VerCodeEndpoint>();

            //services.TryAddScoped<IAMProvider>();
            //services.TryAddScoped<IUserStore<TUser>, IAMUserStore<TUser>>();
        }

        /// <summary>
        /// 使用IAM服务
        /// </summary>
        /// <param name="builder"></param>
        public static void UseIAMService(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<IAMMiddleware>();
        }

        public static EndpointBase GetIAMEndpoint(this HttpContext context)
        {
            var endpoints = context.RequestServices.GetRequiredService<IEnumerable<EndpointBase>>();
            foreach (var endpoint in endpoints)
            {
                 var path = endpoint.Path;
                 if (context.Request.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                 {
                     return endpoint;
                 }
            }

            return null;
        }
    }
}
