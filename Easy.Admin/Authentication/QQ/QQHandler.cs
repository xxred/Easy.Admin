using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Easy.Admin.Authentication.QQ
{
    public class QQHandler : OAuthHandler<QQOptions>
    {
        public QQHandler(IOptionsMonitor<QQOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            // 首先获取OpenId
            var openIdEndpoint = QueryHelpers.AddQueryString(Options.OpenIdEndpoint,
                "access_token", tokens.AccessToken);
            var openIdResponse = await Backchannel.GetAsync(openIdEndpoint, Context.RequestAborted);
            var openIdContent = await openIdResponse.Content.ReadAsStringAsync();
            openIdContent = openIdContent.TrimStart("callback( ").TrimEnd(" );\n");
            var openIdPayload = JObject.Parse(openIdContent);

            // 存储openid，绑定到系统的用户，作为系统在第三方的唯一标识
            var openId = openIdPayload["openid"].Value<string>();

            var tokenRequestParameters = new Dictionary<string, string>()
            {
                { "access_token", tokens.AccessToken },
                { "oauth_consumer_key", Options.ClientId },
                { "openid", openId },
            };

            var endpoint = QueryHelpers.AddQueryString(Options.UserInformationEndpoint, tokenRequestParameters);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, endpoint);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await Backchannel.SendAsync(requestMessage, Context.RequestAborted);

            //var endpoint = QueryHelpers.AddQueryString(Options.UserInformationEndpoint,
            //    "access_token", tokens.AccessToken);
            //var response = await Backchannel.GetAsync(endpoint, Context.RequestAborted);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"An error occurred when retrieving Facebook user information ({response.StatusCode}). Please check if the authentication information is correct and the corresponding Facebook Graph API is enabled.");
            }

            var jObj = JObject.Parse(await response.Content.ReadAsStringAsync());
            // 填充openid
            jObj["openid"] = openId;

            using (var payload = JsonDocument.Parse(jObj.ToString()))// 
            {
                var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme,
                    Options, Backchannel, tokens, payload.RootElement);

                context.RunClaimActions();
                await Events.CreatingTicket(context);
                return new AuthenticationTicket(context.Principal, properties, Scheme.Name);
            }
        }

        /// <summary>
        /// 重载此方法主要是请求回来的token格式是"access_token=xxx&abc=123"的形式，需要另做处理
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override async Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
        {
            var tokenRequestParameters = new Dictionary<string, string>()
            {
                { "client_id", Options.ClientId },
                { "redirect_uri", context.RedirectUri },
                { "client_secret", Options.ClientSecret },
                { "code", context.Code },
                { "grant_type", "authorization_code" },
            };

            // PKCE https://tools.ietf.org/html/rfc7636#section-4.5, see BuildChallengeUrl
            if (context.Properties.Items.TryGetValue(OAuthConstants.CodeVerifierKey, out var codeVerifier))
            {
                tokenRequestParameters.Add(OAuthConstants.CodeVerifierKey, codeVerifier);
                context.Properties.Items.Remove(OAuthConstants.CodeVerifierKey);
            }

            var requestContent = new FormUrlEncodedContent(tokenRequestParameters);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, Options.TokenEndpoint);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestMessage.Content = requestContent;
            var response = await Backchannel.SendAsync(requestMessage, Context.RequestAborted);
            if (response.IsSuccessStatusCode)
            {
                var jObj = await ConvertContentToJObject(response);
                var payload = JsonDocument.Parse(jObj.ToString());
                return OAuthTokenResponse.Success(payload);
            }
            else
            {
                var error = "OAuth token endpoint failure: " + await Display(response);
                return OAuthTokenResponse.Failed(new Exception(error));
            }
        }

        private static async Task<string> Display(HttpResponseMessage response)
        {
            var output = new StringBuilder();
            output.Append("Status: " + response.StatusCode + ";");
            output.Append("Headers: " + response.Headers.ToString() + ";");
            output.Append("Body: " + await response.Content.ReadAsStringAsync() + ";");
            return output.ToString();
        }

        private static async Task<JObject> ConvertContentToJObject(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"An error occurred when retrieving QQ user information ({response.StatusCode}). Please check if the authentication information is correct and the corresponding QQ API is enabled.");
            }

            var content = await response.Content.ReadAsStringAsync();
            var queries = QueryHelpers.ParseQuery(content);
            var payload = new JObject();
            foreach (var query in queries)
            {
                payload[query.Key] = query.Value.ToString();
            }

            return payload;
        }
    }
}
