using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Easy.Admin.Identity.IAM.Endpoints
{
    /// <summary>
    /// 获取验证码端点
    /// </summary>
    public class GetUserInfoEndpoint : EndpointBase
    {
        public override string Path => "/api/Account/GetUserInfo";
        
        public GetUserInfoEndpoint(IAMOptions options, IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions) : base(options, mvcNewtonsoftJsonOptions)
        {
        }
    }
}