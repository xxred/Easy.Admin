using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Areas.Admin.ResponseParams;
using Easy.Admin.Authentication.IAM;
using Easy.Admin.Authentication.OAuthSignIn;
using Easy.Admin.Entities;
using Easy.Admin.Identity.IAM;
using Easy.Admin.Localization;
using Easy.Admin.Localization.Resources;
using Easy.Admin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NewLife;
using NewLife.Log;
using NewLife.Serialization;
using RestSharp;
using XCode.Membership;

namespace Easy.Admin.Authentication.JwtBearer
{
    public class IAMAuthenticationHandler : SignInAuthenticationHandler<IAMOptions>
    {
        private readonly IStringLocalizer<Request> _requestLocalizer;
        private readonly IUserService _userService;
        private const string Idp = "IdentityServer4";

        public IAMAuthenticationHandler(IOptionsMonitor<IAMOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, IStringLocalizer<Request> requestLocalizer, IUserService userService)
            : base(options, logger, encoder, clock)
        {
            _requestLocalizer = requestLocalizer;
            _userService = userService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // 认证之前检查Cookie中是否携带Token，有则设置头部的Authorization
            var authorization = "Authorization";
            if (!Request.Headers.ContainsKey(authorization) && Request.Cookies.TryGetValue("Admin-Token", out var token))
            {
                Request.Headers.Add(authorization, token);
            }
            else
            {
                token = Request.Headers[authorization];
            }

            if (token.IsNullOrWhiteSpace())
            {
                return AuthenticateResult.NoResult();
            }

            var uc = CheckLocalRecord(token);

            if (uc != null)
            {
                return uc.Expire < DateTime.Now ? AuthenticateResult.Fail(_requestLocalizer["No login or login timeout"])
                    : GetAuthenticateResult(uc);
            }

            // 登录记录不存在，请求授权中心
            var result = await Authenticate(token);
            if (result.Status != 0)
            {
                throw ApiException.Common(result.Msg);
            }

            return await LocalSignIn(result.Data, token);
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
            return Task.CompletedTask;
        }

        /// <summary>
        /// 检查本地是否有登录记录
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private UserConnect CheckLocalRecord(string token)
        {
            var uc = UserConnect.FindByAccessToken(token);

            return uc;
        }

        /// <summary>
        /// 获取认证结果
        /// </summary>
        /// <param name="uc"></param>
        /// <returns></returns>
        private AuthenticateResult GetAuthenticateResult(UserConnect uc)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(OAuthSignInAuthenticationDefaults.Sub,uc.UserID.ToString()),
            }));

            var ticket = new AuthenticationTicket(user, Idp);

            return AuthenticateResult.Success(ticket);
        }

        /// <summary>
        /// 向IAM中心请求认证
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<ApiResult<ResponseUserInfo>> Authenticate(string token)
        {
            var restClient = new RestClient(Options.Url);
            restClient.AddDefaultHeader("Authorization", token);
            var path = "/api/Account/GetUserInfo";
            var restRequest = new RestRequest(path);
            var restResponse = await restClient.ExecuteAsync<ApiResult<ResponseUserInfo>>(restRequest);

            if (restResponse.StatusCode != HttpStatusCode.OK)
            {
                XTrace.WriteLine(restResponse.Data.ToJson());
                throw ApiException.Common(_requestLocalizer["Server error"], 500);
            }

            var result = restResponse.Data;

            return result;
        }

        /// <summary>
        /// 创建登录记录
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<AuthenticateResult> LocalSignIn(ResponseUserInfo userInfo, string token)
        {
            var u = await UpdateUserAsync(userInfo);
            var uc = UserConnect.FindByProviderAndOpenID(Idp, userInfo.Name) ?? new UserConnect
            {
                Provider = Idp,
                UserID = u.ID,
                OpenID = userInfo.Name,
                LinkID = userInfo.ID.ToInt(),
                Enable = true
            };

            uc.AccessToken = token;
            uc.Avatar = userInfo.Avatar;
            uc.NickName = userInfo.DisplayName;
            uc.Expire = GetExpire(token);
            uc.Save();

            return GetAuthenticateResult(uc);
        }

        /// <summary>
        /// 更新用户信息，不存在则创建
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        private async Task<IUser> UpdateUserAsync(ResponseUserInfo userInfo)
        {
            var user = await _userService.FindByNameAsync(userInfo.Name);
            var names = new[] { nameof(IUser.Name), nameof(IUser.DisplayName), nameof(IUser.Avatar), nameof(IUser.Sex), };
            var values = new object[] { userInfo.Name, userInfo.DisplayName, userInfo.Avatar, userInfo.Sex };

            if (user == null)
            {
                await _userService.CreateAsync(names, values);
            }
            else
            {
                await _userService.UpdateAsync(names, values);
            }

            user = await _userService.FindByNameAsync(userInfo.Name);

            return user;
        }

        /// <summary>
        /// 获取过期时间
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private DateTime GetExpire(string token)
        {
            token = token.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var exp = jwtToken.Claims.FirstOrDefault(f => f.Type == "exp");

            if (exp == null)
            {
                throw ApiException.Common(_requestLocalizer["Could not find the exp claim in token"], 500);
            }

            var d = new DateTime(1970, 1, 1, 0, 0, 0, DateTime.Now.Kind).AddSeconds(exp.Value.ToInt());

            return d;
        }
    }
}
