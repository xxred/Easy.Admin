using System;
using Easy.Admin.Authentication.QQ;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FacebookAuthenticationOptionsExtensions
    {
        public static AuthenticationBuilder AddQQ(this AuthenticationBuilder builder)
            => builder.AddQQ(QQDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddQQ(this AuthenticationBuilder builder, Action<QQOptions> configureOptions)
            => builder.AddQQ(QQDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddQQ(this AuthenticationBuilder builder, string authenticationScheme, Action<QQOptions> configureOptions)
            => builder.AddQQ(authenticationScheme, QQDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddQQ(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<QQOptions> configureOptions)
            => builder.AddOAuth<QQOptions, QQHandler>(authenticationScheme, displayName, configureOptions);
    }
}
