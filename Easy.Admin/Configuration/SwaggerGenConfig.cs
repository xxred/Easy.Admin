using System;
using System.Collections.Generic;
using System.IO;
using Easy.Admin.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// https://github.com/domaindrivendev/Swashbuckle.AspNetCore
    /// </summary>
    public static class SwaggerGenConfig
    {
        public static void ConfigSwaggerGen(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var oAuthConfiguration = serviceProvider.GetRequiredService<OAuthConfiguration>();
            
            // 文档
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = config["ApiTitle"] ?? "Easy.Admin API", Version = "v1" });

                // 添加注释说明

                var files = Directory.GetFiles(env.ContentRootPath, "*.xml");

                foreach (var file in files)
                {
                    c.IncludeXmlComments(file);
                }

                // 添加控制器注释
                c.TagActionsBy(api =>
                {
                    var controllerActionDescriptor = (ControllerActionDescriptor)api.ActionDescriptor;
                    var tag = controllerActionDescriptor.ControllerName + "-" + controllerActionDescriptor.ControllerTypeInfo.GetDisplayName();
                    return new List<string>()
                    {
                        tag
                    };
                });

                var bearerScheme = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                };

                c.AddSecurityDefinition("Bearer", bearerScheme);

                // 这个要加上，否则请求的时候头部不会带Authorization
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    { bearerScheme, new List<string>()}

                }); ;

                if (!oAuthConfiguration.Authority.IsNullOrEmpty())
                {
                    var oauth2Scheme = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Description = "OAuth2登陆授权",
                        Flows = new OpenApiOAuthFlows()
                        {
                            AuthorizationCode = new OpenApiOAuthFlow()
                            {
                                AuthorizationUrl = new Uri(oAuthConfiguration.Authority.EnsureEnd("/") + "connect/authorize"),
                                TokenUrl = new Uri(
                                    //oAuthConfiguration.Authority.EnsureEnd("/") + "connect/token"
                                    "/api/Account/GetToken", UriKind.Relative
                                    ),
                                Scopes = new Dictionary<string, string>
                                {
                                    {"openid", "唯一标识"},
                                }
                            }
                        }
                    };

                    c.AddSecurityDefinition("oauth2", oauth2Scheme);
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        { oauth2Scheme, new List<string>(){"openid"}}
                    });
                }
            });

        }
    }
}
