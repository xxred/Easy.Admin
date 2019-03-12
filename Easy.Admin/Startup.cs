using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Easy.Admin.Authentication;
using Easy.Admin.Authentication.Github;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

            services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    // options.UserInteraction.ConsentUrl // 同意url，同意授权，给用户勾选
                    options.UserInteraction.LoginReturnUrlParameter = "returnUrl";//返回url的参数名
                    options.UserInteraction.LoginUrl = "/Admin/Account/Login?username=admin&password=123456";

                    options.Authentication.CookieAuthenticationScheme = JwtBearerAuthenticationDefaults.AuthenticationScheme;
                })
                .AddInMemoryIdentityResources(new List<IdentityResource>()
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile()

                })
                .AddInMemoryApiResources(new List<ApiResource>()
                {
                    new ApiResource("api1", "IdentityServer4授权中心")
                })
                .AddInMemoryClients(new List<Client>()
                {
                    new Client
                    {
                        ClientId = "client",

                        // no interactive user, use the clientid/secret for authentication
                        AllowedGrantTypes = GrantTypes.Code,

                        // secret for authentication
                        ClientSecrets =
                        {
                            new Secret("client".Sha256())
                        },

                        // scopes that client has access to
                        AllowedScopes =
                        {
                            "api1",
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                        },
                        RequireConsent = false,

                        RedirectUris = {"https://localhost:44336/sign-client"},

                    },
                    // resource owner password grant client
                    new Client
                    {
                        ClientId = "ro.client",
                        AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                        ClientSecrets =
                        {
                            new Secret("secret".Sha256())
                        },
                        AllowedScopes = {"api1"}
                    },
                    // OpenID Connect hybrid flow client (MVC)
                    new Client
                    {
                        ClientId = "mvc",
                        ClientName = "MVC Client",
                        AllowedGrantTypes = GrantTypes.Hybrid,

                        ClientSecrets =
                        {
                            new Secret("secret".Sha256())
                        },

                        RedirectUris = {"http://localhost:5002/signin-oidc"},
                        PostLogoutRedirectUris = {"http://localhost:5002/signout-callback-oidc"},

                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            "api1"
                        },

                        AllowOfflineAccess = true
                    },
                    // JavaScript Client
                    new Client
                    {
                        ClientId = "js",
                        ClientName = "JavaScript Client",
                        AllowedGrantTypes = GrantTypes.Code,
                        RequirePkce = true,
                        RequireClientSecret = false,

                        RedirectUris = {"http://localhost:5003/callback.html"},
                        PostLogoutRedirectUris = {"http://localhost:5003/index.html"},
                        AllowedCorsOrigins = {"http://localhost:5003"},

                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            "api1"
                        }
                    },
                    ///////////////////////////////////////////
                    // Device Flow Sample
                    //////////////////////////////////////////
                    new Client
                    {
                        ClientId = "device",
                        ClientName = "Device Flow Client",

                        AllowedGrantTypes = GrantTypes.DeviceFlow,
                        RequireClientSecret = false,

                        AllowOfflineAccess = true,

                        AllowedCorsOrigins = {"*"}, // JS test client only

                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.Email,
                            "api1", "api2.read_only", "api2.full_access"
                        }
                    }
                })
                .AddDeveloperSigningCredential()
                .AddJwtBearerClientAuthentication();

            // 身份验证
            services.AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultSignInScheme = JwtBearerAuthenticationDefaults.AuthenticationScheme;
                    })
                // 默认使用Jwt认证
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

                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                })
                .AddOpenIdConnect("IdentityServer4", options =>
                {
                    options.Authority = "https://localhost:44336/";
                    options.ClientId = "client";
                    options.ClientSecret = "client";
                    options.ResponseType = "code";
                    options.SaveTokens = true;

                    options.CallbackPath = "/sign-client";
                    options.SignInScheme = JwtBearerAuthenticationDefaults.AuthenticationScheme;

                    options.Scope.Add("api1");
                });

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

            // 跨域
            app.UseCors(config =>
                config.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .AllowCredentials());

            // 身份认证
            //app.UseAuthentication();

            app.UseIdentityServer();

            app.UseMvc();
        }
    }
}
