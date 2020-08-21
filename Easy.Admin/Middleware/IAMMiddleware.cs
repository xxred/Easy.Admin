using Easy.Admin.Identity.IAM;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Easy.Admin.Authentication.IAM;

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
                var endpoint = context.GetIAMEndpoint();
                if (endpoint != null)
                {
                    await endpoint.ProcessAsync(context);

                    return;
                }

                // 请求不是注册登录相关的，在这里向授权中心认证token
                await context.AuthenticateAsync(IAMAuthenticationDefaults.AuthenticationScheme);
            }

            await _next(context);
        }
    }
}
