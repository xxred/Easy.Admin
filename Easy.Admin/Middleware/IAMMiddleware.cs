using Easy.Admin.Identity.IAM;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Easy.Admin.Authentication.IAM;
using NewLife.Log;

namespace Easy.Admin.Middleware
{
    public class IAMMiddleware
    {
        private readonly RequestDelegate _next;

        public IAMMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAMOptions options)
        {
            if (options.UseIAM)
            {
                XTrace.WriteLine("UseIAM");
                var endpoint = context.GetIAMEndpoint();
                if (endpoint != null)
                {
                    XTrace.WriteLine($"GetIAMEndpoint: {endpoint.Path}");

                    await endpoint.ProcessAsync(context);

                    return;
                }

                XTrace.WriteLine($"向授权中心认证：{context.Request.Path}");

                // 请求不是注册登录相关的，在这里向授权中心认证token
                var authenticateResult = await context.AuthenticateAsync(IAMAuthenticationDefaults.AuthenticationScheme);
                if (authenticateResult?.Principal != null)
                    context.User = authenticateResult.Principal;
            }

            await _next(context);
        }
    }
}
