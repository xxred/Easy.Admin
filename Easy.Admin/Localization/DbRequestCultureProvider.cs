using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Easy.Admin.Authentication.JwtBearer;
using Easy.Admin.Authentication.OAuthSignIn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Easy.Admin.Localization
{
    public class DbRequestCultureProvider : RequestCultureProvider
    {
        private static readonly Task<ProviderCultureResult> NullProviderCultureResult = Task.FromResult((ProviderCultureResult)null);

        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            // 判断请求是否带上了token
            const string authorization = "Authorization";
            var jwtBearerAuthenticationOptions =
                httpContext.RequestServices.GetRequiredService<IOptions<JwtBearerAuthenticationOptions>>();
            var tokenKey = jwtBearerAuthenticationOptions.Value.TokenKey;
            string token = null;
            if (httpContext.Request.Headers.TryGetValue(authorization, out var t1))
            {
                token = t1;
            }
            else if (httpContext.Request.Cookies.TryGetValue(tokenKey, out var t2))
            {
                token = t2;
            }

            if (token == null)
            {
                return NullProviderCultureResult;
            }

            // 解析token，获取语言
            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", "", true, null));
            var lang = jwtSecurityToken.Claims.FirstOrDefault(f => f.Type == OAuthSignInAuthenticationDefaults.Lang)?.Value;

            return lang != null ? Task.FromResult(new ProviderCultureResult((StringSegment)lang)) : NullProviderCultureResult;
        }
    }
}
