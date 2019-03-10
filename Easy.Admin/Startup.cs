using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Easy.Admin.Authentication;
using Easy.Admin.Authentication.Github;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

namespace Easy.Admin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 添加基础Startup
            services.AddTransient<IStartupFilter, BasicStartupFilter>();

            // 身份验证
            services.AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme = "Bearer";
                        options.DefaultSignInScheme = JwtBearerAuthenticationDefaults.AuthenticationScheme;
                    }
                    )
                .AddJwtBearerSignIn()
                //.AddCookie(options =>
                //{
                //    options.LoginPath = "/Account/Login";
                //    options.AccessDeniedPath = "/Account/Forbidden/";
                //})
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;

                    var secretKey = "EasyAdminEasyAdminEasyAdmin";
                    var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));


                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,

                        ValidateIssuer = true,
                        ValidIssuer = "EasyAdminUser",

                        ValidateAudience = true,
                        ValidAudience = "EasyAdminAudience",

                        ValidateLifetime = true,

                        ClockSkew = TimeSpan.Zero
                    };
                })
                .AddGithub(options =>
                {
                    options.ClientId = "c93fcad8f3d8e1ee8997";
                    options.ClientSecret = "00feb809af81a0f98e2e8e767677ca25f1696129";

                    //options.Scope.Add();

                    options.SignInScheme = JwtBearerAuthenticationDefaults.AuthenticationScheme;

                })
                //.AddOpenIdConnect("QQ",options=>
                //{
                //    //options.Authority 
                //})
                ;

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // 文档
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Easy.Admin API", Version = "v1" });
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
            // 跨域
            services.AddCors();
            // 运行情况检查
            services.AddHealthChecks()
                .AddCheck(name: "example",
                    check: () => HealthCheckResult.Healthy("ok123"),
                    //(IHealthCheck)null,
                    //failureStatus:HealthStatus.Unhealthy,
                    tags: new[] { "example" });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Easy.Admin API V1");
                    c.InjectJavascript("/swagger.js");//注入js
                });
                app.UseDeveloperExceptionPage();
                app.UseStaticFiles();
            }
            else
            {
                app.UseHsts();
            }

            // Http跳转Https
            app.UseHttpsRedirection();

            // 身份认证
            app.UseAuthentication();

            // 跨域
            app.UseCors(config =>
                config.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .AllowCredentials());

            app.UseMvc();
        }
    }
}
