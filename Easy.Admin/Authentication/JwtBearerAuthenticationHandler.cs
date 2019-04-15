using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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
            if (Request.Cookies.TryGetValue("Admin-Token", out var token))
            {
                Request.Headers["Authorization"] = token;
                var authenticateResult = await Context.AuthenticateAsync("Bearer");

                if (!authenticateResult.Succeeded) return authenticateResult;

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

            var jwtToken = new JwtToken{ Token = encodedToken};

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
