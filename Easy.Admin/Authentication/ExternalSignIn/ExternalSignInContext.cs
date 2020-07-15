using Easy.Admin.Localization.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Easy.Admin.Authentication.ExternalSignIn
{
    /// <summary>
    /// 移动端登录处理上下文
    /// </summary>
    public class ExternalSignInContext
    {
        public ExternalSignInContext(IHttpClientFactory httpClientFactory, ExternalSignInHandler context, IServiceProvider requestServices,
            IStringLocalizer<Request> requestLocalizer, ClaimsPrincipal user, AuthenticationProperties authenticationProperties)
        {
            HttpClientFactory = httpClientFactory;
            Context = context;
            RequestLocalizer = requestLocalizer;
            User = user;
            AuthenticationProperties = authenticationProperties;
            RequestServices = requestServices;
        }

        public IHttpClientFactory HttpClientFactory { get; }
        public ExternalSignInHandler Context { get; }
        public IServiceProvider RequestServices { get; }
        public IStringLocalizer<Request> RequestLocalizer { get; }
        public ClaimsPrincipal User { get; }
        public AuthenticationProperties AuthenticationProperties { get; }
    }
}
