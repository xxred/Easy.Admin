using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.RequestParams;
using Easy.Admin.Authentication.IAM;
using Easy.Admin.Entities;
using Easy.Admin.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            if (apiResult!=null && apiResult.Data)
            {
                await UpdateUserInfo(context);
            }

            await ExecuteAsync(context);
        }

        private async Task UpdateUserInfo(HttpContext context)
        {
            var req = context.Request;
            var b = new byte[req.ContentLength.Value];

            var total = await req.Body.ReadAsync(b);
            var s = Encoding.UTF8.GetString(b);
            var requestUserInfo = JsonConvert.DeserializeObject<RequestUserInfo>(s);

           await _userService.UpdateAsync(requestUserInfo.ToDictionary());
        }
    }
}