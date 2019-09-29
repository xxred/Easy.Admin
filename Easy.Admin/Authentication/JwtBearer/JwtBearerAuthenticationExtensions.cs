using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Easy.Admin.Authentication;
using Easy.Admin.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NewLife.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JwtBearerAuthenticationExtensions
    {
        public static AuthenticationBuilder AddJwtBearerSignIn(this AuthenticationBuilder builder)
            => builder.AddJwtBearerSignIn(JwtBearerAuthenticationDefaults.AuthenticationScheme);

        public static AuthenticationBuilder AddJwtBearerSignIn(this AuthenticationBuilder builder, string authenticationScheme)
            => builder.AddJwtBearerSignIn(authenticationScheme, configureOptions: null);

        public static AuthenticationBuilder AddJwtBearerSignIn(this AuthenticationBuilder builder, Action<JwtBearerAuthenticationOptions> configureOptions)
            => builder.AddJwtBearerSignIn(JwtBearerAuthenticationDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddJwtBearerSignIn(this AuthenticationBuilder builder, string authenticationScheme, Action<JwtBearerAuthenticationOptions> configureOptions)
            => builder.AddJwtBearerSignIn(authenticationScheme, displayName: null, configureOptions: configureOptions);

        public static AuthenticationBuilder AddJwtBearerSignIn(this AuthenticationBuilder builder, string authenticationScheme,
            string displayName, Action<JwtBearerAuthenticationOptions> configureOptions)
        {
            builder.AddJwtBearer(authenticationScheme + JwtBearerDefaults.AuthenticationScheme, options =>
              {
                // 复制一份给JwtBearerOptions
                var opts = new JwtBearerAuthenticationOptions();
                  configureOptions?.Invoke(opts);
                  options.Copy(opts,false,
                      //nameof(JwtBearerAuthenticationOptions.ExpireTimeSpan),
                      nameof(JwtBearerAuthenticationOptions.Events)
                      );
              });

            return builder.AddScheme<JwtBearerAuthenticationOptions, JwtBearerAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
