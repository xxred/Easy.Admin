using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Authentication.JwtBearer;
using Easy.Admin.Entities;
using Easy.Admin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NewLife.Log;
using NewLife.Model;
using XCode.Membership;

namespace Easy.Admin.Authentication.OAuthSignIn
{
    /// <summary>
    /// 第三方登录登出
    /// </summary>
    public class OAuthSignInAuthenticationHandler : SignInAuthenticationHandler<JwtBearerAuthenticationOptions>
    {
        private readonly IUserService _userService;


        public OAuthSignInAuthenticationHandler(IOptionsMonitor<JwtBearerAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, IUserService userService) : base(options, logger, encoder, clock)
        {
            _userService = userService;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        protected override async Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            var options = Options;
            var tvps = options.TokenValidationParameters;
            var signingKey = tvps.IssuerSigningKey;
            var handler = new JwtSecurityTokenHandler();
            var newTokenExpiration = DateTime.Now.Add(options.ExpireTimeSpan);
            // TODO 优化第三方登录是绑定或创建用户
            var appUser = await _userService.GetOrCreateUserAsync(user, properties, options.CreateUserOnOAuthLogin);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(OAuthSignInAuthenticationDefaults.Sub, appUser.ID.ToString()),
                new Claim(OAuthSignInAuthenticationDefaults.UniqueName, appUser.Name)
            }, properties.Items["scheme"]);

            var securityToken = handler.CreateToken(new SecurityTokenDescriptor()
            {
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                Subject = identity,
                Expires = newTokenExpiration,
                Issuer = tvps.ValidateIssuer ? tvps.ValidIssuer : null,
                Audience = tvps.ValidateAudience ? tvps.ValidAudience : null,
            });

            var tokenValue = handler.WriteToken(securityToken);

            var encodedToken = "Bearer " + tokenValue;

            var jwtToken = new JwtToken { Token = encodedToken };

            Context.Features[typeof(JwtToken)] = jwtToken;
            properties.Items[nameof(JwtToken)] = encodedToken;

            Response.Cookies.Append(options.TokenKey, encodedToken);

            var returnUrlKey = options.ReturnUrlKey;
            if (properties.Items.TryGetValue(returnUrlKey, out var returnUrl))
            {
                Response.Cookies.Append(returnUrlKey, returnUrl);
            }
        }

        protected override Task HandleSignOutAsync(AuthenticationProperties properties)
        {
            // throw new NotImplementedException();
            return Task.CompletedTask;
        }
    }
}
