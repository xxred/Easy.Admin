using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Easy.Admin.Services;
using Easy.Admin.Services.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NewLife.Caching;

namespace Easy.Admin.Common
{
    public static class CommonServicesExtensions
    {
        /// <summary>
        /// 添加默认文件上传模块
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            services.AddSingleton<ICache, MemoryCache>();
            services.TryAddSingleton<IRedisService, RedisService>();

            return services;
        }
    }
}
