using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using Easy.Admin.Authentication;
using Easy.Admin.Configuration;
using Easy.Admin.ModelBinders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

namespace Easy.Admin
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            // 清空所有ClaimType映射，不进行任何转换
            //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 添加数据库连接
            services.AddConnectionStr();

            // 添加身份认证
            services.AddIdentity(options =>
            {
                options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
                options.ClaimsIdentity.UserNameClaimType = JwtRegisteredClaimNames.UniqueName;
            });

            // 身份验证
            services
                .AddAuthentication(
                    options =>
                    {
                        options.DefaultScheme = IdentityConstants.ApplicationScheme;
                        //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                        options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
                    })
                //.AddIdentityServerAuthentication()
                // SignManager内部使用IdentityConstants.ApplicationScheme作为登陆方案名称
                .AddJwtBearerSignIn(IdentityConstants.ApplicationScheme, options =>
                {
                    options.SaveToken = true;

                    var secretKey = Configuration["BearerSecretKey"]
                                    ?? JwtBearerAuthenticationDefaults.BearerSecretKey;
                    var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));


                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,

                        ValidateIssuer = false,
                        //ValidIssuer = Configuration["ValidIssuer"]
                        //              ?? JwtBearerAuthenticationDefaults.ValidIssuer,

                        ValidateAudience = false,
                        //ValidAudience = Configuration["ValidAudience"]
                        //                ?? JwtBearerAuthenticationDefaults.ValidAudience,

                        ValidateLifetime = true,

                        ValidateTokenReplay = false,

                        ClockSkew = TimeSpan.Zero
                    };
                })
                .AddJwtBearerSignIn(IdentityConstants.ExternalScheme)
                .AddJwtBearerSignIn(IdentityConstants.TwoFactorRememberMeScheme)
                .AddJwtBearerSignIn(IdentityConstants.TwoFactorUserIdScheme)
                // IdentityServer内部Cookie登陆方案名称，避免跟正常使用的JwtBearerSignIn方案名称一致，
                // TODO 一样的话将会验证多一些声明，具体未详细记录
                //.AddJwtBearerSignIn("Jwt-Cookie")
                .AddOpenIdConnect("IdentityServer4", options =>
                {
                    options.Authority = "https://localhost:44352";
                    options.ClientId = "client";
                    options.ClientSecret = "client";
                    options.ResponseType = "code";
                    options.SaveTokens = true;

                    options.CallbackPath = "/sign-client";
                    options.SignInScheme = IdentityConstants.ApplicationScheme;

                    options.Scope.Add("api1");
                });

            services.AddSingleton<OAuthConfiguration>();

            services.AddMvc(options =>
            {
                options.ModelBinderProviders.Insert(0, new PagerModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new EntityModelBinderProvider());
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            .ConfigJsonOptions();

            // 文档
            services.ConfigSwaggerGen();

            // 跨域
            services.AddCors();

            // 扫描控制器添加菜单
            services.ScanController();
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Easy.Admin API V1");
                //c.InjectJavascript("/swagger.js");//注入js
            });

            app.UseDefaultFiles();

            app.UseStaticFiles();

            // 跨域
            app.UseCors(config =>
                config.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .AllowCredentials());

            // 身份认证
            app.UseAuthentication();

            app.UseMvc();

            app.UseSpa(config =>
            {
                if (Environment.IsDevelopment())
                {
                    // 下面的代理可以在本地开发使用，线上部署使用StaticFile，设置默认页和文件地址
                    config.UseProxyToSpaDevelopmentServer(Configuration["SpaDevServer"] ?? "http://127.0.0.1:1337/");
                }
                else
                {
                    // 这个设置用来处理所有前面没有命中的请求，
                    // 比如 /indexl.html(返回默认主页文件的内容，但是前端没有这个路由)、/(返回默认主页文件的内容)、/404(返回默认主页文件的内容)等
                    // 总的来说就是返回默认主页的的内容，而路由由前端处理，
                    // UseSpaStaticFiles返回前端文件，可以设置相对目录，比如这里是放在dist文件夹，前端无需处理
                    if (!Environment.WebRootPath.IsNullOrWhiteSpace())
                    {
                        config.Options.DefaultPageStaticFileOptions = new StaticFileOptions();
                        config.Options.DefaultPageStaticFileOptions.FileProvider =
                        new PhysicalFileProvider(Path.Combine(Environment.WebRootPath, "dist"));
                    }
                }
            });
        }
    }
}
