using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
using NewLife.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Easy.Admin.Identity.IAM.Endpoints
{
    /// <summary>
    /// 获取验证码端点
    /// </summary>
    public class DeleteAccountEndpoint : EndpointBase
    {
        private readonly IUserService _userService;

        public override string Path => "/api/Account/DeleteAccount";
        
        public DeleteAccountEndpoint(IAMOptions options, IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions, IUserService userService) : base(options, mvcNewtonsoftJsonOptions)
        {
            _userService = userService;
        }

        public override async Task ProcessAsync(HttpContext context)
        {
            await HandleRequestAsync(context);

            var resp = RestResponse;

            var apiResult = JsonConvert.DeserializeObject<ApiResult<bool?>>(resp.Content);
            XTrace.WriteLine($"删除账号返回结果{apiResult.ToJson()}");
            if (apiResult?.Data != null && apiResult.Data.Value)
            {
                XTrace.WriteLine($"执行删除账号");

                await DeleteUserInfo(context);
            }
            XTrace.WriteLine($"删除账号完成");

            await ExecuteAsync(context);
        }

        private async Task DeleteUserInfo(HttpContext context)
        {
            // 本地登录
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
            XTrace.WriteLine($"本地账号id:{id}");

            var user = await _userService.FindByIdAsync(id);
            XTrace.WriteLine($"本地账号名称:{user.Name}");

            await _userService.DeleteAccountAsync(user);
        }
    }
}