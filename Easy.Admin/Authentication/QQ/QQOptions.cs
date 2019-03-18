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

            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            //ClaimActions.MapJsonSubKey("urn:facebook:age_range_min", "age_range", "min");
            //ClaimActions.MapJsonSubKey("urn:facebook:age_range_max", "age_range", "max");
            ClaimActions.MapJsonKey(ClaimTypes.DateOfBirth, "birthday");
            ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
            ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
            ClaimActions.MapJsonKey(ClaimTypes.GivenName, "first_name");
            //ClaimActions.MapJsonKey("urn:facebook:middle_name", "middle_name");
            ClaimActions.MapJsonKey(ClaimTypes.Surname, "last_name");
            ClaimActions.MapJsonKey(ClaimTypes.Gender, "gender");
            //ClaimActions.MapJsonKey("urn:facebook:link", "link");
            //ClaimActions.MapJsonSubKey("urn:facebook:location", "location", "name");
            ClaimActions.MapJsonKey(ClaimTypes.Locality, "locale");
            //ClaimActions.MapJsonKey("urn:facebook:timezone", "timezone");
        }

        /// <summary>
        /// 获取OpenId的链接
        /// </summary>
        public string OpenIdEndpoint { get; set; }

    }
}
