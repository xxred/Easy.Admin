using System.Threading.Tasks;
using Easy.Admin.Authentication.IAM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Easy.Admin.Identity.IAM.Endpoints
{
    /// <summary>
    /// 注册端点
    /// </summary>
    public class RegisterEndpoint : EndpointBase
    {
        public override string Path => "/api/Account/Register";
        
        public RegisterEndpoint(IAMOptions options, IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions) : base(options, mvcNewtonsoftJsonOptions)
        {
        }
    }
}