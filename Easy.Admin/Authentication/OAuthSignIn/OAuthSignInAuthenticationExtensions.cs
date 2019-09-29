using System;
using Easy.Admin.Authentication.JwtBearer;
using Easy.Admin.Authentication.OAuthSignIn;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OAuthSignInAuthenticationExtensions
    {
        public static AuthenticationBuilder AddOAuthSignIn(this AuthenticationBuilder builder)
            => builder.AddOAuthSignIn(OAuthSignInAuthenticationDefaults.AuthenticationScheme);

        public static AuthenticationBuilder AddOAuthSignIn(this AuthenticationBuilder builder, string authenticationScheme)
            => builder.AddOAuthSignIn(authenticationScheme, configureOptions: null);

        public static AuthenticationBuilder AddOAuthSignIn(this AuthenticationBuilder builder, Action<JwtBearerAuthenticationOptions> configureOptions)
            => builder.AddOAuthSignIn(OAuthSignInAuthenticationDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddOAuthSignIn(this AuthenticationBuilder builder, string authenticationScheme, Action<JwtBearerAuthenticationOptions> configureOptions)
            => builder.AddOAuthSignIn(authenticationScheme, displayName: null, configureOptions: configureOptions);

        public static AuthenticationBuilder AddOAuthSignIn(this AuthenticationBuilder builder, string authenticationScheme,
            string displayName, Action<JwtBearerAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<JwtBearerAuthenticationOptions, OAuthSignInAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
