using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Easy.Admin.Authentication.IAM;
using Easy.Admin.Authentication.JwtBearer;
using Easy.Admin.Authentication.IAMAuthentication;
using Easy.Admin.Authentication.OAuthSignIn;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Easy.Admin.Authentication.IAMAuthentication
{
    public static class IAMAuthenticationExtensions
    {
        public static AuthenticationBuilder AddIAMAuthentication(this AuthenticationBuilder builder)
            => builder.AddIAMAuthentication(IAMAuthenticationDefaults.AuthenticationScheme);

        public static AuthenticationBuilder AddIAMAuthentication(this AuthenticationBuilder builder, string authenticationScheme)
            => builder.AddIAMAuthentication(authenticationScheme, configureOptions: null);

        public static AuthenticationBuilder AddIAMAuthentication(this AuthenticationBuilder builder, Action<IAMOptions> configureOptions)
            => builder.AddIAMAuthentication(IAMAuthenticationDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddIAMAuthentication(this AuthenticationBuilder builder, string authenticationScheme, Action<IAMOptions> configureOptions)
            => builder.AddIAMAuthentication(authenticationScheme, displayName: null, configureOptions: configureOptions);

        public static AuthenticationBuilder AddIAMAuthentication(this AuthenticationBuilder builder, string authenticationScheme,
            string displayName, Action<IAMOptions> configureOptions)
        {
            return builder.AddScheme<IAMOptions, IAMAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
