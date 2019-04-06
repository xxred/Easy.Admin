using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Easy.Admin.Authentication;
using Easy.Admin.Authentication.Github;
using Easy.Admin.Authentication.QQ;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

namespace Easy.Admin
{
    public class Startup
    {
        public Startup(IConfiguration configuration,IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 添加基础Startup
            services.AddTransient<IStartupFilter, BasicStartupFilter>();

            services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    // options.UserInteraction.ConsentUrl // 同意url，同意授权，给用户勾选
                    options.UserInteraction.LoginReturnUrlParameter = "returnUrl";//返回url的参数名
                    //options.UserInteraction.LoginUrl = "/Admin/Account/Login?username=admin&password=123456";
                    options.UserInteraction.LoginUrl = "/login";


                    options.Authentication.CookieAuthenticationScheme = "Jwt-Cookie";
                })
                //.AddInMemoryIdentityResources(new List<IdentityResource>()
                //{
                //    new IdentityResources.OpenId(),
                //    new IdentityResources.Profile()

                //})
                //.AddInMemoryApiResources(new List<ApiResource>()
                //{
                //    new ApiResource("api1", "IdentityServer4授权中心")
                //})
                //.AddInMemoryClients(new List<Client>()
                //{
                //    new Client
                //    {
                //        ClientId = "client",

                //        // no interactive user, use the clientid/secret for authentication
                //        AllowedGrantTypes = GrantTypes.Code,

                //        // secret for authentication
                //        ClientSecrets =
                //        {
                //            new Secret("client".Sha256())
                //        },

                //        // scopes that client has access to
                //        AllowedScopes =
                //        {
                //            "api1",
                //            IdentityServerConstants.StandardScopes.OpenId,
                //            IdentityServerConstants.StandardScopes.Profile,
                //        },
                //        RequireConsent = false,

                //        RedirectUris = {"https://localhost:44336/sign-client"},

                //    },
                //    // resource owner password grant client
                //    new Client
                //    {
                //        ClientId = "ro.client",
                //        AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                //        ClientSecrets =
                //        {
                //            new Secret("secret".Sha256())
                //        },
                //        AllowedScopes = {"api1"}
                //    },
                //    // OpenID Connect hybrid flow client (MVC)
                //    new Client
                //    {
                //        ClientId = "mvc",
                //        ClientName = "MVC Client",
                //        AllowedGrantTypes = GrantTypes.Hybrid,

                //        ClientSecrets =
                //        {
                //            new Secret("secret".Sha256())
                //        },

                //        RedirectUris = {"http://localhost:5002/signin-oidc"},
                //        PostLogoutRedirectUris = {"http://localhost:5002/signout-callback-oidc"},

                //        AllowedScopes =
                //        {
                //            IdentityServerConstants.StandardScopes.OpenId,
                //            IdentityServerConstants.StandardScopes.Profile,
                //            "api1"
                //        },

                //        AllowOfflineAccess = true
                //    },
                //    // JavaScript Client
                //    new Client
                //    {
                //        ClientId = "js",
                //        ClientName = "JavaScript Client",
                //        AllowedGrantTypes = GrantTypes.Code,
                //        RequirePkce = true,
                //        RequireClientSecret = false,

                //        RedirectUris = {"http://localhost:5003/callback.html"},
                //        PostLogoutRedirectUris = {"http://localhost:5003/index.html"},
                //        AllowedCorsOrigins = {"http://localhost:5003"},

                //        AllowedScopes =
                //        {
                //            IdentityServerConstants.StandardScopes.OpenId,
                //            IdentityServerConstants.StandardScopes.Profile,
                //            "api1"
                //        }
                //    },
                //    ///////////////////////////////////////////
                //    // Device Flow Sample
                //    //////////////////////////////////////////
                //    new Client
                //    {
                //        ClientId = "device",
                //        ClientName = "Device Flow Client",

                //        AllowedGrantTypes = GrantTypes.DeviceFlow,
                //        RequireClientSecret = false,

                //        AllowOfflineAccess = true,

                //        AllowedCorsOrigins = {"*"}, // JS test client only

                //        AllowedScopes =
                //        {
                //            IdentityServerConstants.StandardScopes.OpenId,
                //            IdentityServerConstants.StandardScopes.Profile,
                //            IdentityServerConstants.StandardScopes.Email,
                //            "api1", "api2.read_only", "api2.full_access"
                //        }
                //    }
                //})
                .AddXCodeConfigurationStore()
                .AddXCodeOperationalStore(options =>
                {
                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = false;
                    // options.TokenCleanupInterval = 15; // interval in seconds. 15 seconds useful for debugging
                })
                .AddDeveloperSigningCredential()
                //.AddJwtBearerClientAuthentication()
                ;

            // 身份验证
            services.AddAuthentication(
                    options =>
                    {
                        options.DefaultScheme = JwtBearerAuthenticationDefaults.AuthenticationScheme;
                        //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultAuthenticateScheme = JwtBearerAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultSignInScheme = JwtBearerAuthenticationDefaults.AuthenticationScheme;
                    })
                // 默认使用Jwt认证
                .AddJwtBearerSignIn()
                .AddJwtBearerSignIn("Jwt-Cookie")
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
                .AddOpenIdConnect("IdentityServer4", options =>
                {
                    options.Authority = "https://localhost:44336";
                    options.ClientId = "client";
                    options.ClientSecret = "client";
                    options.ResponseType = "code";
                    options.SaveTokens = true;

                    options.CallbackPath = "/sign-client";
                    options.SignInScheme = JwtBearerAuthenticationDefaults.AuthenticationScheme;

                    options.Scope.Add("api1");
                })
                .AddQQ(options =>
                {
                    options.ClientId = "101554717";
                    options.ClientSecret = "fa819f1077ecbdffedbefb1f63039d9f";
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

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

            services.AddSpaStaticFiles(options =>
            {
                options.RootPath = Path.Combine(Environment.WebRootPath, "dist");
            });

            //services.UseAdminUI();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {

                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Http跳转Https
            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Easy.Admin API V1");
                //c.InjectJavascript("/swagger.js");//注入js
            });

            app.UseDefaultFiles();

            app.UseStaticFiles();
            //app.UseSpaStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath, "dist"))
            //});

            // 跨域
            app.UseCors(config =>
                config.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .AllowCredentials());

            // 身份认证
            //app.UseAuthentication();

            app.UseIdentityServer();

            app
                //.UseMvcWithDefaultRoute();//
            .UseMvc();

            //app.UseAdminUI();

            app.UseSpa(config =>
            {
                //SpaStaticFilesOptions
                //config.Options.SourcePath = Path.Combine(env.WebRootPath, "dist");

                // 下面的代理可以在本地开发使用，线上部署使用StaticFile，设置默认页和文件地址
                config.UseProxyToSpaDevelopmentServer("http://127.0.0.1:1337/");

                // 这个设置用来处理所有前面没有命中的请求，
                // 比如 /indexl.html(返回默认主页文件的内容，但是前端没有这个路由)、/(返回默认主页文件的内容)、/404(返回默认主页文件的内容)等
                // 总的来说就是返回默认主页的的内容，而路由由前端处理，
                // UseSpaStaticFiles返回前端文件，可以设置相对目录，比如这里是放在dist文件夹，前端无需处理
                //config.Options.DefaultPageStaticFileOptions = new StaticFileOptions();
                //config.Options.DefaultPageStaticFileOptions.FileProvider =
                //new PhysicalFileProvider(Path.Combine(env.WebRootPath, "dist"));
            });
        }
    }
}
