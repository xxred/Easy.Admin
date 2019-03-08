// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.Extensions.DependencyInjection;

namespace Easy.Admin.Authentication.QQ
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
