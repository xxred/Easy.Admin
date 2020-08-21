using System.Threading.Tasks;
using Easy.Admin.Authentication.IAM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Easy.Admin.Identity.IAM.Endpoints
{
    /// <summary>
    /// 获取验证码端点
    /// </summary>
    public class VerCodeEndpoint : EndpointBase
    {
        public override string Path => "/api/Account/GetVerCode";
        
        public VerCodeEndpoint(IAMOptions options, IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions) : base(options, mvcNewtonsoftJsonOptions)
        {
        }
    }
}