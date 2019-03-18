// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Facebook;

namespace Easy.Admin.Authentication.QQ
{
    /// <summary>
    /// Configuration options for <see cref="QQHandler"/>.
    /// </summary>
    public class QQOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new <see cref="QQOptions"/>.
        /// </summary>
        public QQOptions()
        {
            CallbackPath = new PathString("/sign-qq");
            AuthorizationEndpoint = QQDefaults.AuthorizationEndpoint;
            TokenEndpoint = QQDefaults.TokenEndpoint;
            OpenIdEndpoint = QQDefaults.OpenIdEndpoint;
            UserInformationEndpoint = QQDefaults.UserInformationEndpoint;

            Scope.Add("get_user_info");

            ClaimActions.MapJsonKey(ClaimTypes.Name, "nickname");
            ClaimActions.MapJsonKey(ClaimTypes.Gender, "gender");
            ClaimActions.MapJsonKey("urn:qq:avatar", "figureurl_qq_2");
        }

        /// <summary>
        /// 获取OpenId的链接
        /// </summary>
        public string OpenIdEndpoint { get; set; }

    }
}
