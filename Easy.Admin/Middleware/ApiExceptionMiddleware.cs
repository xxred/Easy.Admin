using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using NewLife.Log;
using Newtonsoft.Json;
using Easy.Admin.Entities;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// 异常拦截处理
    /// </summary>
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// 异常拦截中间件
        /// </summary>
        /// <param name="next"></param>
        public ApiExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        /// <summary>
        /// InvokeAsync
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                //记录系统的异常信息
                XTrace.WriteException(e);

                // 如果已经开始响应到客户端，直接抛出异常，否则下面写入响应也会抛异常
                if (context.Response.HasStarted)
                {
                    throw;
                }

                var data = new ApiResult<String>();

                if (e is ApiException apiexc)
                {
                    data.Status = apiexc.Status;
                    data.Msg = apiexc.Message;
                    context.Response.StatusCode = 200;
                }
                else
                {
                    data.Status = 500;
                    data.Msg = e.Message;
                    context.Response.StatusCode = (Int32)HttpStatusCode.InternalServerError;
                }

                context.Response.Headers.Add(HeaderNames.ContentType, "application/json;charset=utf-8");
                await context.Response.WriteAsync(JsonConvert.SerializeObject(data), Encoding.UTF8);
            }

        }
    }

    public static class MiddlewareExtension
    {
        public static IApplicationBuilder UseApiExceptionHandler(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiExceptionMiddleware>();
        }
    }

}
