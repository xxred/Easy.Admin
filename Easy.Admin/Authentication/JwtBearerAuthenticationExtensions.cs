using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Easy.Admin.Authentication
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

        public static AuthenticationBuilder AddJwtBearerSignIn(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<JwtBearerAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<JwtBearerAuthenticationOptions, JwtBearerAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
