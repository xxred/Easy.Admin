using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Easy.Admin.Authentication.IAM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Easy.Admin.Identity.IAM.Endpoints
{
    public class EndpointBase
    {
        private readonly IRestClient _restClient;
        protected IRestResponse RestResponse;
        
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

        public virtual async Task ProcessAsync(HttpContext context)
        {
            await HandleRequestAsync(context);
            await ExecuteAsync(context);
        }

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
                var body = JObject.Parse(s);
                restRequest.AddJsonBody(body);
            }

            RestResponse = await _restClient.ExecuteAsync(restRequest);
        }

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