using Easy.Admin.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Threading.Tasks;
using Easy.Admin.Authentication.IAM;
using XCode.Membership;

namespace Easy.Admin.Identity.IAM
{
    public class IAMProvider
    {
        private const string Prefix = "api";
        private readonly IRestClient _restClient;
        //private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAMOptions _options;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public IAMProvider(IHttpContextAccessor httpContextAccessor, IAMOptions options, IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions)
        {
            //_httpContextAccessor = httpContextAccessor;
            var options2 = mvcNewtonsoftJsonOptions?.Value ?? throw new ArgumentNullException(nameof(mvcNewtonsoftJsonOptions));
            _jsonSerializerSettings = options2.SerializerSettings;
            _options = options;
            _restClient = new RestClient(_options.Url);
            _restClient.UseNewtonsoftJson(_jsonSerializerSettings);

            if (httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                _restClient.AddDefaultHeader("Authorization", token);
            }
        }

        public virtual async Task<IdentityResult> CreateAsync(IUser user)
        {
            var req = new RestRequest($"{Prefix}/User", DataFormat.Json)
                .AddJsonBody(user);
            //req.UseNewtonsoftJson(_jsonSerializerSettings);

            var resp = await _restClient.PostAsync<ApiResult<string>>(req);

            if (resp.Status == 0)
            {
                return IdentityResult.Success;
            }

            return IdentityResult.Failed(new IdentityError() { Code = "402", Description = resp.Msg });
        }
    }
}
