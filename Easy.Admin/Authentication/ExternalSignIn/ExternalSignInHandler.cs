using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Easy.Admin.Authentication.JwtBearer;
using Easy.Admin.Authentication.OAuthSignIn;
using Easy.Admin.Authentication.QQ;
using Easy.Admin.Entities;
using Easy.Admin.Localization.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewLife.Reflection;
using Newtonsoft.Json.Linq;

namespace Easy.Admin.Authentication.ExternalSignIn
{
    /// <summary>
    /// 外部登录，用于移动端凭借token登录
    /// </summary>
    public class ExternalSignInHandler : SignInAuthenticationHandler<JwtBearerAuthenticationOptions>
    {
        private readonly IHttpClientFactory _clientFactory;
        private IStringLocalizer<Request> _requestLocalizer;
        private IEnumerable<IExternalSignInHandler> _handlers;

        public ExternalSignInHandler(IOptionsMonitor<JwtBearerAuthenticationOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IHttpClientFactory clientFactory,
            IStringLocalizer<Request> requestLocalizer, IEnumerable<IExternalSignInHandler> externalSignInHandlers)
            : base(options, logger, encoder, clock)
        {
            _clientFactory = clientFactory;
            _requestLocalizer = requestLocalizer;
            _handlers = externalSignInHandlers;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return await Task.FromResult(AuthenticateResult.NoResult());
        }

        protected override Task HandleSignOutAsync(AuthenticationProperties properties)
        {
            return Task.CompletedTask;
        }

        protected override async Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            var scheme = properties.Items["scheme"];
            //var providerKey = properties.Items["providerKey"];

            var externalSignInContext = new ExternalSignInContext(_clientFactory, this, this.Context.RequestServices, _requestLocalizer, user, properties);

            foreach (var handler in _handlers)
            {
                if (handler.CheckName(scheme))
                {
                    await handler.Handle(externalSignInContext);
                    break;
                }
            }

            await Context.SignInAsync(OAuthSignInAuthenticationDefaults.AuthenticationScheme, user, properties);
        }

        public void RunClaimActions(ClaimActionCollection claimActions, ClaimsIdentity claimsIdentity, JsonElement userData)
        {
            foreach (var action in claimActions)
            {
                action.Run(userData, claimsIdentity, Options.ClaimsIssuer ?? Scheme.Name);
            }
        }
    }
}
