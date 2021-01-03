using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Easy.Admin.Authentication.IAM;
using Easy.Admin.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NewLife;
using NewLife.Log;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Easy.Admin.Identity.IAM.Endpoints
{
    public class EndpointBase
    {
        private readonly IRestClient _restClient;
        protected IRestResponse RestResponse;
        protected JObject Body;

        /// <summary>
        /// 请求路径
        /// </summary>
        public virtual string Path { get; }

        public EndpointBase(IAMOptions options, IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (mvcNewtonsoftJsonOptions == null) throw new ArgumentNullException(nameof(mvcNewtonsoftJsonOptions));

            _restClient = new RestClient(options.Url);
            _restClient.UseNewtonsoftJson(mvcNewtonsoftJsonOptions.Value.SerializerSettings);
        }

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task ProcessAsync(HttpContext context)
        {
            await HandleRequestAsync(context);
            await ExecuteAsync(context);
        }

        /// <summary>
        /// 处理请求参数
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected async Task HandleRequestAsync(HttpContext context)
        {
            SetAuthorization(context);

            var req = context.Request;

            var restRequest = new RestRequest(Path, DataFormat.Json);
            if (Enum.TryParse(req.Method, true, out Method method))
            {
                restRequest.Method = method;
            }

            if (req.Query.Any())
            {
                foreach (var (key, value) in req.Query)
                {
                    restRequest.AddQueryParameter(key, value);
                }
            }

            if (req.ContentLength != null)
            {
                var b = new byte[req.ContentLength.Value];

                var total = await req.Body.ReadAsync(b);
                var s = Encoding.UTF8.GetString(b);
                if (!s.IsNullOrWhiteSpace())
                {
                    Body = JObject.Parse(s);
                    restRequest.AddJsonBody(Body);
                }
            }

            RestResponse = await _restClient.ExecuteAsync(restRequest);

            // 判断是否有异常
            if (RestResponse.ErrorException != null)
            {
                XTrace.WriteException(RestResponse.ErrorException);
                throw ApiException.Common("The IAM Server is not available", 500);
            }
        }

        /// <summary>
        /// 返回结果
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected async Task ExecuteAsync(HttpContext context)
        {
            context.Response.Headers.Add(HeaderNames.ContentType, "application/json;charset=utf-8");
            await context.Response.WriteAsync(RestResponse.Content, Encoding.UTF8);
        }

        private void SetAuthorization(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var token))
            {
                _restClient.AddDefaultHeader("Authorization", token);
            }
        }
    }
}