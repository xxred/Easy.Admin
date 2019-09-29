using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Authentication.JwtBearer;
using Easy.Admin.Entities;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public OAuthSignInAuthenticationHandler(IOptionsMonitor<JwtBearerAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, UserManager<ApplicationUser> userManager) : base(options, logger, encoder, clock)
        {
            _userManager = userManager;
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
            var appUser = await GetOrCreateUserAsync(user, properties);

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

        /// <summary>
        /// 获取或创建用户
        /// </summary>
        /// <returns></returns>
        private async Task<IManageUser> GetOrCreateUserAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            var options = Options;
            var provider = properties.Items["scheme"];
            var openid = user.FindFirstValue(OAuthSignInAuthenticationDefaults.Sub);

            var uc = UserConnect.FindByProviderAndOpenID(provider, openid);

            IManageUser appUser;

            if (uc == null)
            {
                if (!options.CreateUserOnOAuthLogin)
                {
                    throw ApiException.Common("用户不存在，请联系管理员");
                }

                uc = new UserConnect() { Provider = provider, OpenID = openid, Enable = true };
                uc.Fill(user);

                appUser = new ApplicationUser { Name = Guid.NewGuid().ToString().Substring(0, 8), Enable = true, RoleID = 4}; // 角色id 4 为游客

                // 此处可改用本系统服务替换，去除ApplicationUser依赖
                var result = await _userManager.CreateAsync(appUser as ApplicationUser, "123456");

                if (!result.Succeeded)
                {
                    throw ApiException.Common($"创建用户失败：{result.Errors.First().Description}");
                }

                uc.UserID = appUser.ID;
            }
            else
            {
                appUser = await _userManager.FindByIdAsync(uc.UserID.ToString());
            }

            if (!appUser.Enable)
            {
                throw ApiException.Common($"用户已被禁用");
            }

            // 填充用户信息
            Fill(appUser, user);

            if (appUser is IAuthUser user3)
            {
                user3.Logins++;
                user3.LastLogin = DateTime.Now;
                user3.LastLoginIP = Request.Host.Host;
                user3.Save();
            }

            try
            {
                uc.Save();
            }
            catch (Exception ex)
            {
                //为了防止某些特殊数据导致的无法正常登录，把所有异常记录到日志当中。忽略错误
                XTrace.WriteException(ex);
            }

            return appUser;
        }

        /// <summary>
        /// 填充用户信息
        /// </summary>
        private void Fill(IManageUser user, ClaimsPrincipal user1)
        {
            if (user is User<ApplicationUser> user2)
            {
                if (user2.Sex == SexKinds.未知 && user1.HasClaim(h => h.Type == OAuthSignInAuthenticationDefaults.Gender))
                {
                    var sex = user1.FindFirstValue(OAuthSignInAuthenticationDefaults.Gender);
                    user2.Sex = sex.Contains("男") ? SexKinds.男 : SexKinds.女;
                }

                if (user2.Avatar.IsNullOrWhiteSpace() && user1.HasClaim(h => h.Type == OAuthSignInAuthenticationDefaults.Avatar))
                {
                    var avatar = user1.FindFirstValue(OAuthSignInAuthenticationDefaults.Avatar);
                    user2.Avatar = avatar;
                }

                if (user2.DisplayName.IsNullOrWhiteSpace() && user1.HasClaim(h => h.Type == OAuthSignInAuthenticationDefaults.GivenName))
                {
                    var givenName = user1.FindFirstValue(OAuthSignInAuthenticationDefaults.GivenName);
                    user2.DisplayName = givenName;
                }
            }
        }
    }
}
