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
        private readonly IAMOptions _options;
        private readonly IRestClient _restClient;

        public IAMMiddleware(RequestDelegate next, IAMOptions options, IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions)
        {
            if (mvcNewtonsoftJsonOptions == null) throw new ArgumentNullException(nameof(mvcNewtonsoftJsonOptions));
            _next = next;
            _options = options;

            _restClient = new RestClient(_options.Url);
            _restClient.UseNewtonsoftJson(mvcNewtonsoftJsonOptions.Value.SerializerSettings);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var req = context.Request;
            var path = req.Path;
            if (_options.ProxyUrlList.Contains(path))
            {
                await _next(context);
                return;
            }

            SetAuthorization(context);

            var restRequest = new RestRequest(req.Path, DataFormat.Json);
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

            var resp = await _restClient.ExecuteAsync<JObject>(restRequest);

            if (resp.Content.Contains("Bearer"))
            {
                SaveToken(resp.Content);
            }

            context.Response.Headers.Add(HeaderNames.ContentType, "application/json;charset=utf-8");
            await context.Response.WriteAsync(resp.Content, Encoding.UTF8);
        }

        private void SetAuthorization(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var token))
            {
                _restClient.AddDefaultHeader("Authorization", token);
            }
        }

        private void SaveToken(string content)
        {
            var jobj = JObject.Parse(content);

            var token = jobj["Data.Token"].ToString();
        }
    }
}
