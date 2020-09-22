using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.RequestParams;
using Easy.Admin.Authentication.IAM;
using Easy.Admin.Entities;
using Easy.Admin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NewLife.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XCode.Membership;

namespace Easy.Admin.Identity.IAM.Endpoints
{
    /// <summary>
    /// 更新用户信息端点
    /// </summary>
    public class UpdateUserInfoEndpoint : EndpointBase
    {
        private readonly IUserService _userService;
        public override string Path => "/api/Account/UpdateUserInfo";

        public UpdateUserInfoEndpoint(IAMOptions options, IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions, IUserService userService) : base(options, mvcNewtonsoftJsonOptions)
        {
            _userService = userService;
        }

        public override async Task ProcessAsync(HttpContext context)
        {
            await HandleRequestAsync(context);

            var resp = RestResponse;

            var apiResult = JsonConvert.DeserializeObject<ApiResult<bool>>(resp.Content);

            if (apiResult != null && apiResult.Data)
            {
                await UpdateUserInfo(context);
            }

            await ExecuteAsync(context);
        }

        private async Task UpdateUserInfo(HttpContext context)
        {
            var req = context.Request;

            var requestUserInfo = JsonConvert.DeserializeObject<RequestUserInfo>(Body.ToString());
            var authenticateResult = await context.AuthenticateAsync(IAMAuthenticationDefaults.AuthenticationScheme);
            if (authenticateResult?.Principal == null)
            {
                XTrace.WriteLine("本地登录失败，更新本地用户信息失败");
                return;
            }

            var id = authenticateResult?.Principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            
            if (id == null)
            {
                XTrace.WriteLine("context.User中找不到存放id的声明");
                return;
            }

            var user = await _userService.FindByIdAsync(id);

            requestUserInfo.ID = user.ID;
            requestUserInfo.Name = user.Name;

            await _userService.UpdateAsync(requestUserInfo.ToDictionary());
        }
    }
}