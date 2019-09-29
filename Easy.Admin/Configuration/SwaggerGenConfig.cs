using System;
using System.Collections.Generic;
using System.IO;
using Easy.Admin.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenConfig
    {
        public static void ConfigSwaggerGen(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var env = serviceProvider.GetRequiredService<IHostingEnvironment>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var oAuthConfiguration = serviceProvider.GetRequiredService<OAuthConfiguration>();
            
            // 文档
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = config["ApiTitle"] ?? "Easy.Admin API", Version = "v1" });

                // 添加注释说明

                var files = Directory.GetFiles(env.ContentRootPath, "*.xml");

                foreach (var file in files)
                {
                    c.IncludeXmlComments(file);
                }

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                
                // 这个要加上，否则请求的时候头部不会带Authorization
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer",new string[]{}}
                });

                if (!oAuthConfiguration.Authority.IsNullOrEmpty())
                {
                    c.AddSecurityDefinition("oauth2", new OAuth2Scheme
                    {
                        Type = "oauth2",
                        Flow = "authorizationCode",
                        TokenUrl = "/api/Account/GetToken",
                        //oAuthConfiguration.Authority.EnsureEnd("/") + "connect/token",
                        AuthorizationUrl = oAuthConfiguration.Authority.EnsureEnd("/") + "connect/authorize",
                        Description = "OAuth2登陆授权",
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid","唯一标识" }
                        }
                    });
                    c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                    {
                        { "oauth2",new string[]{}}
                    });
                }
            });

        }
    }
}
