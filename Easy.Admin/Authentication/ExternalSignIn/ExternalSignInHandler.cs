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

        public ExternalSignInHandler(IOptionsMonitor<JwtBearerAuthenticationOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IHttpClientFactory clientFactory, IStringLocalizer<Request> requestLocalizer)
            : base(options, logger, encoder, clock)
        {
            _clientFactory = clientFactory;
            _requestLocalizer = requestLocalizer;
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
            
            // TODO 此处做成可拓展，通过注入获取
            switch (scheme)
            {
                case "QQ":
                    await QQHandler(user, properties);
                    break;
                default:
                    throw ApiException.Common(_requestLocalizer["The login type is not supported"]);
            }

            await Context.SignInAsync(OAuthSignInAuthenticationDefaults.AuthenticationScheme, user, properties);
        }

        private async Task QQHandler(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            var providerKey = properties.Items["providerKey"];

            // 首先获取OpenId
            var openIdEndpoint = QueryHelpers.AddQueryString(QQDefaults.OpenIdEndpoint,
                "access_token", providerKey);
            var backchannel = _clientFactory.CreateClient();

            var openIdResponse = await backchannel.GetAsync(openIdEndpoint);
            var openIdContent = await openIdResponse.Content.ReadAsStringAsync();
            openIdContent = openIdContent.TrimStart("callback( ").TrimEnd(" );\n");
            var openIdPayload = JObject.Parse(openIdContent);

            // 存储openid，绑定到系统的用户，作为系统在第三方的唯一标识
            var openId = openIdPayload["openid"].Value<string>();
            var clientId = openIdPayload["client_id"].Value<string>();
            var tokenRequestParameters = new Dictionary<string, string>()
            {
                { "access_token", providerKey },
                { "oauth_consumer_key", clientId },
                { "openid", openId },
            };
            var endpoint = QueryHelpers.AddQueryString(QQDefaults.UserInformationEndpoint, tokenRequestParameters);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, endpoint);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await backchannel.SendAsync(requestMessage);
            var userInfoPayload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            var ret = userInfoPayload.RootElement.GetString("ret").ToInt();
            if (ret < 0)
            {
                throw ApiException.Common(_requestLocalizer[userInfoPayload.RootElement.GetString("msg")]);
            }

            var options = Context.RequestServices.GetRequiredService<IOptionsMonitor<QQOptions>>().CurrentValue;

            var identity = user.Identity as ClaimsIdentity;
            RunClaimActions(options.ClaimActions, identity, userInfoPayload.RootElement);

            identity?.AddClaim(new Claim(OAuthSignInAuthenticationDefaults.Sub, openId));
        }

        private void RunClaimActions(ClaimActionCollection claimActions, ClaimsIdentity claimsIdentity, JsonElement userData)
        {
            foreach (var action in claimActions)
            {
                action.Run(userData, claimsIdentity, Options.ClaimsIssuer ?? Scheme.Name);
            }
        }
    }
}
