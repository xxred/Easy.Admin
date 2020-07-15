using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Easy.Admin.Authentication.JwtBearer;
using Easy.Admin.Authentication.ExternalSignIn;
using Easy.Admin.Authentication.OAuthSignIn;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Easy.Admin.Authentication.ExternalSignIn
{
    public static class ExternalSignInExtensions
    {
        public static AuthenticationBuilder AddExternalSignIn(this AuthenticationBuilder builder)
            => builder.AddExternalSignIn(ExternalSignInDefaults.AuthenticationScheme);

        public static AuthenticationBuilder AddExternalSignIn(this AuthenticationBuilder builder, string authenticationScheme)
            => builder.AddExternalSignIn(authenticationScheme, configureOptions: null);

        public static AuthenticationBuilder AddExternalSignIn(this AuthenticationBuilder builder, Action<JwtBearerAuthenticationOptions> configureOptions)
            => builder.AddExternalSignIn(ExternalSignInDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddExternalSignIn(this AuthenticationBuilder builder, string authenticationScheme, Action<JwtBearerAuthenticationOptions> configureOptions)
            => builder.AddExternalSignIn(authenticationScheme, displayName: null, configureOptions: configureOptions);

        public static AuthenticationBuilder AddExternalSignIn(this AuthenticationBuilder builder, string authenticationScheme,
            string displayName, Action<JwtBearerAuthenticationOptions> configureOptions)
        {
            builder.Services.TryAddSingleton<IExternalSignInHandler, ExternalSignInQQHandler>();
            return builder.AddScheme<JwtBearerAuthenticationOptions, ExternalSignInHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
