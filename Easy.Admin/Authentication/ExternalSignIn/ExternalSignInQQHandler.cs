using Easy.Admin.Authentication.OAuthSignIn;
using Easy.Admin.Authentication.QQ;
using Easy.Admin.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Easy.Admin.Authentication.ExternalSignIn
{
    public class ExternalSignInQQHandler : IExternalSignInHandler
    {
        public bool CheckName(string name) => name == "QQ";

        public async Task Handle(ExternalSignInContext externalSignInContext)
        {
            var properties = externalSignInContext.AuthenticationProperties;
            var clientFactory = externalSignInContext.HttpClientFactory;
            var requestLocalizer = externalSignInContext.RequestLocalizer;
            var externalSignInHandler = externalSignInContext.Context;
            var user = externalSignInContext.User;
            var requestServices = externalSignInContext.RequestServices;
            var providerKey = properties.Items["providerKey"];

            // 首先获取OpenId
            var openIdEndpoint = QueryHelpers.AddQueryString(QQDefaults.OpenIdEndpoint,
                "access_token", providerKey);
            var backchannel = clientFactory.CreateClient();

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
                throw ApiException.Common(requestLocalizer[userInfoPayload.RootElement.GetString("msg")]);
            }

            var options = requestServices.GetRequiredService<IOptionsMonitor<QQOptions>>().CurrentValue;

            var identity = user.Identity as ClaimsIdentity;
            externalSignInHandler.RunClaimActions(options.ClaimActions, identity, userInfoPayload.RootElement);

            identity?.AddClaim(new Claim(OAuthSignInAuthenticationDefaults.Sub, openId));
        }
    }
}
