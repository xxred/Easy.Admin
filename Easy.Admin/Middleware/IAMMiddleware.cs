using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easy.Admin.Identity.IAM;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

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

            }

            await _next(context);
        }
    }
}
