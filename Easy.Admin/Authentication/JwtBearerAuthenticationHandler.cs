using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace Easy.Admin.Authentication
{
    public class JwtBearerAuthenticationHandler : SignInAuthenticationHandler<JwtBearerAuthenticationOptions>
    {
        public JwtBearerAuthenticationHandler(IOptionsMonitor<JwtBearerAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Cookies.TryGetValue("token", out var token))
            {
                Request.Headers["Authorization"] = token;
                var authenticateResult = await Context.AuthenticateAsync("Bearer");

                if (!authenticateResult.Succeeded) return authenticateResult;

                var claims = authenticateResult.Principal.Claims;
                // sub 唯一标识，用于identityServer，
                // 同时它也应该具有具有唯一标识，ClaimTypes.NameIdentity
                var hasSub = authenticateResult.Principal.HasClaim(h => h.Type == "sub");
                if (!hasSub)
                {
                    if (authenticateResult.Principal.HasClaim(h => h.Type == ClaimTypes.NameIdentifier))
                    {
                        ClaimsIdentity claimsIdentity;
                        if ((claimsIdentity = authenticateResult.Principal.Identity as ClaimsIdentity) != null)
                        {
                            var sub = authenticateResult.Principal.FindFirst(f => f.Type == ClaimTypes.NameIdentifier)
                                .Value;
                            claimsIdentity.AddClaim(new Claim("sub", sub));
                        }
                    }
                    if (authenticateResult.Principal.HasClaim(h => h.Type == "http://schemas.microsoft.com/identity/claims/identityprovider"))
                    {
                        ClaimsIdentity claimsIdentity;
                        if ((claimsIdentity = authenticateResult.Principal.Identity as ClaimsIdentity) != null)
                        {
                            var sub = authenticateResult.Principal.FindFirst(f => f.Type == "http://schemas.microsoft.com/identity/claims/identityprovider")
                                .Value;
                            claimsIdentity.AddClaim(new Claim("idp", sub));
                        }
                    }
                }

                return authenticateResult;
            }

            return AuthenticateResult.Fail("Cookie中没有发现token");
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

            var secretKey = "EasyAdminEasyAdminEasyAdmin";
            var signingKey = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                SecurityAlgorithms.HmacSha256);

            var securityToken = handler.CreateToken(new SecurityTokenDescriptor()
            {
                SigningCredentials = signingKey,
                Subject = identity,
                Expires = newTokenExpiration,
                Issuer = "EasyAdminUser",
                Audience = "EasyAdminAudience",
            });

            var encodedToken = "Bearer " + handler.WriteToken(securityToken);

            Response.Cookies.Append("token", encodedToken);

            if (properties.Items.ContainsKey("returnUrl"))
            {
                Response.Cookies.Append("returnUrl", properties.Items["returnUrl"]);
            }

            return Task.CompletedTask;
        }
    }
}
