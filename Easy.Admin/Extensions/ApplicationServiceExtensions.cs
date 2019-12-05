using Easy.Admin.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Easy.Admin.Extensions
{
    public static class ApplicationServiceExtensions
    {
        /// <summary>
        /// 添加用户服务
        /// </summary>
        /// <typeparam name="TUSer"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUserServiceCollection<TUSer>(this IServiceCollection services)
        {
            services.TryAddScoped(typeof(IUserService), typeof(UserService<>).MakeGenericType(typeof(TUSer)));

            return services;
        }
    }
}
