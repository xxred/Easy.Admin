using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices;

namespace Easy.Admin.SpaServices
{
    public static class VueDevelopmentServerMiddlewareExtensions
    {
        /// <summary>
        ///通过将请求传递到 vue 应用服务器的实例来处理请求。
        ///这意味着您可以随时提供最新的CLI构建的资源，而不必
        ///手动运行vue app server。
        /// 此功能应仅在开发中使用。对于生产部署，请确保不启用vue app server。
        /// </summary>
        /// <param name="spaBuilder">The <see cref="ISpaBuilder"/>.</param>
        /// <param name="npmScript">The name of the script in your package.json file that launches the create-react-app server.</param>
        public static void UseVueDevelopmentServer(
            this ISpaBuilder spaBuilder,
            string npmExe,
            string scriptName)
        {
            if (spaBuilder == null)
            {
                throw new ArgumentNullException(nameof(spaBuilder));
            }

            var spaOptions = spaBuilder.Options;

            if (string.IsNullOrEmpty(spaOptions.SourcePath))
            {
                throw new InvalidOperationException($"To use {nameof(UseVueDevelopmentServer)}, you must supply a non-empty value for the {nameof(SpaOptions.SourcePath)} property of {nameof(SpaOptions)} when calling {nameof(SpaApplicationBuilderExtensions.UseSpa)}.");
            }

            VueDevelopmentServerMiddleware.Attach(spaBuilder, npmExe, scriptName);
        }
    }
}

