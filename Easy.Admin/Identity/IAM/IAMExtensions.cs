using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using XCode.Membership;

namespace Easy.Admin.Identity.IAM
{
    public static class IAMExtensions
    {
        public static void AddIAMService<TUser>(this IServiceCollection services, Action<IAMOptions> configure)
        where TUser : User<TUser>, new()
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.Configure(configure);
            services.TryAddScoped<IAMProvider>();
            services.TryAddScoped<IUserStore<TUser>, IAMUserStore<TUser>>();
        }
    }
}
