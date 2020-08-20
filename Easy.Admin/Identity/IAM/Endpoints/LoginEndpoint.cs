using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Entities;
using Easy.Admin.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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


        public LoginEndpoint(IAMOptions options, IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions, IUserService userService)
            : base(options, mvcNewtonsoftJsonOptions)
        {
            _userService = userService;
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
            var jwtToken = JsonConvert.DeserializeObject<JwtToken>(content);
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
            uc.Expire = jwtToken.Expires ?? DateTime.Now.AddMinutes(15);
            uc.Save();
        }

        private async Task<IUser> UpdateUserAsync(UserInfo userInfo)
        {
            var user = await _userService.FindByNameAsync(userInfo.Name);
            var names = new[] { nameof(IUser.Name), nameof(IUser.DisplayName), nameof(IUser.Avatar), nameof(IUser.Sex), };
            var values = new object[] { userInfo.Name, userInfo.NickName, userInfo.Avatar, userInfo.Gender };

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
    }
}