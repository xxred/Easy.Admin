using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Configuration;
using Easy.Admin.ModelBinders;
using Easy.Admin.SpaServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Easy.Admin
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            // 清空所有ClaimType映射，不进行任何转换
            // JwtBearer认证后会将简短的声明转成长声明
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 添加数据库连接
            services.AddConnectionStr();

            // 添加身份标识Identity
            services.AddAdminIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
                options.ClaimsIdentity.UserNameClaimType = JwtRegisteredClaimNames.UniqueName;
            });

            // 添加身份验证
            services.ConfigAuthentication();

            services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new PagerModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new EntityModelBinderProvider());
            })
            .ConfigJsonOptions();

            // 文档
            services.ConfigSwaggerGen();

            // 跨域
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                });
            });

            // 扫描控制器添加菜单
            services.ScanController();

            // 添加HttpClient
            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseApiExceptionHandler();

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Http跳转Https
            //app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                var oAuthConfiguration = app.ApplicationServices.GetRequiredService<OAuthConfiguration>();
                c.SwaggerEndpoint("/swagger/v1/swagger.json", Configuration["ApiTitle"] ?? "EasyAdmin API");
                //c.InjectJavascript("/swagger.js");//注入js

                if (!oAuthConfiguration.Authority.IsNullOrEmpty())
                {
                    c.OAuthClientId(oAuthConfiguration.ClientId);
                    c.OAuthClientSecret(oAuthConfiguration.ClientSecret);
                    //c.OAuthRealm("test-realm");
                    c.OAuthAppName(Configuration["ApiTitle"]);
                    //c.OAuthScopeSeparator(" ");
                    //c.OAuthAdditionalQueryStringParams(new { foo = "bar" });
                    c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
                }
            });

            app.UseDefaultFiles();

            app.UseStaticFiles();

            app.UseRouting();

            // 跨域
            app.UseCors("default");

            // 身份认证
            app.UseAuthentication();

            // 授权
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // 以下为SPA准备
            if (Environment.WebRootPath != null)
            {


                app.UseSpa(options =>
                {
                    if (Environment.IsDevelopment())
                    {
                        // 如果前端源码路径配置为空，说明不需要代理服务
                        var clientAppSourcePath = Configuration["ClientAppSourcePath"];
                        if (clientAppSourcePath.IsNullOrWhiteSpace())
                        {
                            return;
                        }

                        // 开发时，当前目录就是项目目录，而不是bin目录
                        options.Options.SourcePath = clientAppSourcePath;
                        //options.Options.StartupTimeout = TimeSpan.FromSeconds(20);
                        options.UseVueDevelopmentServer("yarn", "start");
                    }
                    else
                    {
                        // 如果dist文件夹不存在，说明没有部署前端文件
                        var dist = Path.Combine(Environment.WebRootPath, "dist");
                        if (!Directory.Exists(dist))
                        {
                            return;
                        }

                        var staticFileOptions = new StaticFileOptions()
                        {
                            FileProvider = new PhysicalFileProvider(dist)
                        };

                        app.UseSpaStaticFiles(staticFileOptions);
                        options.Options.DefaultPageStaticFileOptions = staticFileOptions;
                    }
                });
            }
        }
    }
}
