using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Easy.Admin.Configuration
{
    public class SwaggerGenConfig
    {
        public static void ConfigSwaggerGen(SwaggerGenOptions c, IHostingEnvironment env)
        {
            c.SwaggerDoc("v1", new Info { Title = "云博智慧农业平台 API", Version = "v1" });

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
        }
    }
}
