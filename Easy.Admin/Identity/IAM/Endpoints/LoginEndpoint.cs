using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Authentication.IAM;
using Easy.Admin.Entities;
using Easy.Admin.Localization.Resources;
using Easy.Admin.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XCode.Membership;

namespace Easy.Admin.Identity.IAM.Endpoints
{
    /// <summary>
    /// 登录端点
    /// </summary>
    public class LoginEndpoint : EndpointBase
    {
        /// <inheritdoc />
        public override string Path => "/api/Account/Login";
        private readonly IUserService _userService;
        private readonly IStringLocalizer<Request> _requestLocalizer;

        public LoginEndpoint(IAMOptions options, IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions,
            IUserService userService, IStringLocalizer<Request> requestLocalizer)
            : base(options, mvcNewtonsoftJsonOptions)
        {
            _userService = userService;
            _requestLocalizer = requestLocalizer;
        }

        public override async Task ProcessAsync(HttpContext context)
        {
            await HandleRequestAsync(context);

            var resp = RestResponse;

            if (resp.Content.Contains("Bearer"))
            {
                await SaveToken(resp.Content);
            }

            await ExecuteAsync(context);
        }

        private async Task SaveToken(string content)
        {
            var jwtTokenResult = JsonConvert.DeserializeObject<ApiResult<JwtToken>>(content);
            if (jwtTokenResult.Status != 0)
            {
                throw ApiException.Common(jwtTokenResult.Msg);
            }

            var jwtToken = jwtTokenResult.Data;

            var idp = "IdentityServer4";
            var userInfo = jwtToken.UserInfo;

            if (userInfo == null)
            {
                throw ApiException.Common("登录返回的UserInfo不能为空", 500);
            }

            var u = await UpdateUserAsync(userInfo);

            var uc = UserConnect.FindByProviderAndOpenID(idp, userInfo.Name) ?? new UserConnect
            {
                Provider = idp,
                UserID = u.ID,
                OpenID = userInfo.Name,
                LinkID = userInfo.UserID.ToInt(),
                Enable = true
            };

            uc.AccessToken = jwtToken.Token;
            uc.Avatar = userInfo.Avatar;
            uc.NickName = userInfo.NickName;
            uc.Expire = jwtToken.Expires ?? GetExpire(jwtToken.Token);
            uc.Save();
        }

        /// <summary>
        /// 更新用户信息，不存在则创建
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        private async Task<IUser> UpdateUserAsync(UserInfo userInfo)
        {
            var user = await _userService.FindByNameAsync(userInfo.Name);
            var names = new[] { nameof(IUser.Name), nameof(IUser.DisplayName), nameof(IUser.Avatar), nameof(IUser.Sex), };
            var values = new object[] { userInfo.Name, userInfo.NickName, userInfo.Avatar, userInfo.Gender };

            if (user == null)
            {
                // 创建用户没有密码会报错
                var nList = new List<string>(names);
                nList.Add(nameof(IUser.Password));
                var vList = new List<object>(values);
                vList.Add("123456");
                await _userService.CreateAsync(nList.ToArray(), vList.ToArray());
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