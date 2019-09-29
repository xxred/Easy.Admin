using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Easy.Admin.Authentication.JwtBearer
{
    public class JwtBearerAuthenticationHandler : SignInAuthenticationHandler<JwtBearerAuthenticationOptions>
    {
        IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;


        public JwtBearerAuthenticationHandler(IOptionsMonitor<JwtBearerAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, IConfiguration configuration, UserManager<ApplicationUser> userManager)
            : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // 认证之前检查Cookie中是否携带Token，有则设置头部的Authorization
            var authorization = "Authorization";
            if (!Request.Headers.ContainsKey(authorization) && Request.Cookies.TryGetValue(Options.TokenKey, out var token))
            {
                Request.Headers.Add(authorization, token);
            }

            var authenticateResult = await Context.AuthenticateAsync(Scheme.Name + JwtBearerDefaults.AuthenticationScheme);

            //// 采用Bearer方案认证，认证失败直接返回，
            //if (!authenticateResult.Succeeded) return authenticateResult;

            // authenticateResult.Principal 后续会被设置到 Context.User，所以在这里直接设置Context.User不生效
            // 也不要在这里设置authenticateResult.Principal，后续中间件可能用到一些声明，所以这里不能设置

            return authenticateResult;
        }

        protected override Task HandleSignOutAsync(AuthenticationProperties properties)
        {
            foreach (var item in Request.Cookies)
            {
                Response.Cookies.Delete(item.Key);
            }

            return Task.CompletedTask;
        }

        protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            var options = this.Options;
            var tvps = options.TokenValidationParameters;
            var signingKey = tvps.IssuerSigningKey;
            var handler = new JwtSecurityTokenHandler();
            var newTokenExpiration = DateTime.Now.Add(options.ExpireTimeSpan);
            var identity = new ClaimsIdentity(user.Identity);

            // 添加IdentityServer需要的声明，如果不是用于IdentityServer，则无需添加以下声明
            // sub 唯一标识，用于IdentityServer，
            // 同时它也应该具有具有唯一标识，ClaimTypes.NameIdentifier
            // 以下简称位于IdentityModel.JwtClaimTypes
            var subject = "sub"; // JwtClaimTypes.Subject
            var identityProvider = "idp"; // JwtClaimTypes.IdentityProvider
            //var issuedAt = "iat"; // JwtClaimTypes.IssuedAt
            var authenticationTime = "auth_time"; // JwtClaimTypes.AuthenticationTime

            if (!user.HasClaim(h => h.Type == subject))
            {
                identity.AddClaim(new Claim(subject, identity.FindFirst(f => f.Type == ClaimTypes.NameIdentifier).Value));
            }

            if (!user.HasClaim(h => h.Type == identityProvider))
            {
                identity.AddClaim(new Claim(identityProvider, "local"));
            }

            if (!user.HasClaim(h => h.Type == authenticationTime))
            {
                identity.AddClaim(new Claim(authenticationTime, DateTime.Now.ToInt() + ""));
            }

            //identity.AddClaim(new Claim(issuedAt, DateTime.Now.ToInt() + "")); // JwtSecurityTokenHandler生成的token自带iat，所以不用

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

            return Task.CompletedTask;
        }
    }
}
