using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace Easy.Admin.Authentication
{
    public class JwtBearerAuthenticationHandler : SignInAuthenticationHandler<JwtBearerAuthenticationOptions>
    {
        IConfiguration _configuration;

        public JwtBearerAuthenticationHandler(IOptionsMonitor<JwtBearerAuthenticationOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IConfiguration configuration)
            : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            /*
             * oauth认证过程需要用到cookie，此处将cookie中的token设置到头部Authorization
             * 然后调用调用bearer方案进行认证，
             */

            if (!Request.Headers.ContainsKey(HeaderNames.Authorization))
            {
                if (!Request.Cookies.TryGetValue("Admin-Token", out var token))
                {
                    return AuthenticateResult.Fail("Cookie中没有发现token项Admin-Token");
                }

                Request.Headers[HeaderNames.Authorization] = token;
            }

            var authenticateResult = await Context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);

            // 采用Bearer方案认证，认证失败直接返回，
            if (!authenticateResult.Succeeded) return authenticateResult;

            /*
             * 以下是为identityServer认证提供必要的声明，可能在identityServer中定义可解决此问题
             * 不应该在这里处理，这里只是暂时这么处理
             */

            // sub 唯一标识，用于identityServer，
            // 同时它也应该具有具有唯一标识，ClaimTypes.NameIdentity
            // 以下简称位于JwtClaimTypes
            var subject = "sub"; // JwtClaimTypes.Subject
            var identityProvider = "idp"; // JwtClaimTypes.IdentityProvider
            var issuedAt = "iat"; // JwtClaimTypes.IssuedAt
            var authenticationTime = "auth_time"; // JwtClaimTypes.AuthenticationTime

            var principal = authenticateResult.Principal;
            var claimsIdentity = principal.Identity as ClaimsIdentity;
            if (claimsIdentity == null)
            {
                return authenticateResult;
            }

            var hasSub = principal.HasClaim(h => h.Type == subject);
            if (!hasSub)
            {
                var subjectClaim = principal.FindFirst(f => f.Type == ClaimTypes.NameIdentifier);
                if (subjectClaim != null)
                {
                    claimsIdentity.AddClaim(new Claim(subject, subjectClaim.Value));
                }

                var identityProviderClaim = authenticateResult.Principal.FindFirst(f =>
                    f.Type == "http://schemas.microsoft.com/identity/claims/identityprovider");
                claimsIdentity.AddClaim(identityProviderClaim != null
                    ? new Claim(identityProvider, identityProviderClaim.Value)
                    : new Claim(identityProvider, "local"));// IdentityServerConstants.LocalIdentityProvider

                var authenticationTimeClaim = principal.FindFirst(f => f.Type == issuedAt);
                if (authenticationTimeClaim != null)
                {
                    claimsIdentity.AddClaim(new Claim(authenticationTime, authenticationTimeClaim.Value));
                }
            }

            return authenticateResult;

        }

        protected override Task HandleSignOutAsync(AuthenticationProperties properties)
        {
            throw new NotImplementedException();
        }

        protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            var handler = new JwtSecurityTokenHandler();
            var newTokenExpiration = DateTime.Now.Add(TimeSpan.FromHours(2));
            var identity = new ClaimsIdentity(user.Identity);

            var secretKey = _configuration["BearerSecretKey"] ?? JwtBearerAuthenticationDefaults.BearerSecretKey;
            var signingKey = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                SecurityAlgorithms.HmacSha256);

            var securityToken = handler.CreateToken(new SecurityTokenDescriptor()
            {
                SigningCredentials = signingKey,
                Subject = identity,
                Expires = newTokenExpiration,
                //Issuer = "EasyAdminUser",
                //Audience = "EasyAdminAudience",
            });

            var encodedToken = "Bearer " + handler.WriteToken(securityToken);

            var jwtToken = new JwtToken { Token = encodedToken };

            Context.Features[typeof(JwtToken)] = jwtToken;
            properties.Items[nameof(JwtToken)] = encodedToken;

            Response.Cookies.Append("Admin-Token", encodedToken);

            if (properties.Items.ContainsKey("returnUrl"))
            {
                Response.Cookies.Append("returnUrl", properties.Items["returnUrl"]);
            }

            return Task.CompletedTask;
        }
    }
}
