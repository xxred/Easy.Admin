using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenConfig
    {
        public static void ConfigSwaggerGen(this IServiceCollection services)
        {
            var env = services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>();

            // 文档
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Easy.Admin API", Version = "v1" });

                // 添加注释说明

                var files = Directory.GetFiles(env.ContentRootPath, "*.xml");

                foreach (var file in files)
                {
                    c.IncludeXmlComments(file);
                }


                //c.AddSecurityDefinition("oauth2", new OAuth2Scheme
                //{
                //    Type = "oauth2",
                //    Flow = "password",
                //    TokenUrl = "/Admin/Account/Login",
                //    Description = "OAuth2登陆授权",
                //    Scopes = new Dictionary<string, string>
                //    {
                //        { "user", "普通用户"}
                //    }
                //});

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                //c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                //{
                //    { "oauth2",new string[]{}}
                //});

                // 这个要加上，否则请求的时候头部不会带Authorization
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer",new string[]{}}
                });
            });

        }
    }
}
