using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;

namespace Easy.Admin.Localization
{
    public static class DbLocalizationServiceCollectionExtensions
    {
        /// <summary>
        /// 添加数据库存储的全球化
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDbLocalization(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IStringLocalizerFactory, DbStringLocalizerFactory>();

            services.Configure<RequestLocalizationOptions>(
                options =>
                {
                    var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo("zh-CN"),
                        new CultureInfo("en-US"),
                        new CultureInfo("de-CH"),
                        new CultureInfo("fr-CH"),
                        new CultureInfo("it-CH")
                    };

                    options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;

                    options.RequestCultureProviders.Clear();
                    options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
                    options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
                    options.RequestCultureProviders.Add(new DbRequestCultureProvider());
                });

            return services;
        }
    }
}
