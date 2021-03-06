using System.Threading.Tasks;
using Easy.Admin.Authentication.IAM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Easy.Admin.Identity.IAM.Endpoints
{
    /// <summary>
    /// 修改密码端点
    /// </summary>
    public class ChangePasswordEndpoint : EndpointBase
    {
        public override string Path => "/api/Account/ChangePassword";
        
        public ChangePasswordEndpoint(IAMOptions options, IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions) : base(options, mvcNewtonsoftJsonOptions)
        {
        }
    }
}