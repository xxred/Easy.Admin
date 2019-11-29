using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Easy.Admin.Authentication;
using Easy.Admin.Authentication.Github;
using Easy.Admin.Authentication.JwtBearer;
using Easy.Admin.Authentication.OAuthSignIn;
using Easy.Admin.Authentication.QQ;
using Easy.Admin.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 认证配置
    /// </summary>
    public static class AuthenticationConfig
    {
        public static IServiceCollection ConfigAuthentication(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

            // 身份验证
            var authenticationBuilder = services
                .AddAuthentication(
                    options =>
                    {
                        options.DefaultScheme = IdentityConstants.ApplicationScheme;
                        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                        options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
                        options.RequireAuthenticatedSignIn = false;
                    })
                // SignManager内部使用IdentityConstants.ApplicationScheme作为登陆方案名称
                // 因此这里全部使用IdentityConstants开头的协议名称
                .AddJwtBearerSignIn(IdentityConstants.ApplicationScheme, options =>
                {
                    ConfigureJwtBearerOptions(options, configuration);
                })
                .AddJwtBearerSignIn(IdentityConstants.ExternalScheme)
                // 处理第三方登录
                .AddOAuthSignIn(OAuthSignInAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    ConfigureJwtBearerOptions(options, configuration);

                    //options.CreateUserOnOAuthLogin = false;
                })
                .AddJwtBearerSignIn(IdentityConstants.TwoFactorRememberMeScheme)
                .AddJwtBearerSignIn(IdentityConstants.TwoFactorUserIdScheme);

            // 从设置中读取并设置OAuth客户端
            SetOAuthClient(authenticationBuilder, configuration);

            return services;
        }

        public static void ConfigureJwtBearerOptions(JwtBearerAuthenticationOptions options, IConfiguration configuration)
        {
            options.SaveToken = true;

            var secretKey = configuration["BearerSecretKey"] ?? JwtBearerAuthenticationDefaults.BearerSecretKey;
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));

            var expireTimeSpan = configuration["Authentication:ExpireMinute"] ?? "90";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(int.Parse(expireTimeSpan));

            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = false,
                //ValidIssuer = configuration["ValidIssuer"]
                //              ?? JwtBearerAuthenticationDefaults.ValidIssuer,

                ValidateAudience = false,
                //ValidAudience = configuration["ValidAudience"]
                //                ?? JwtBearerAuthenticationDefaults.ValidAudience,

                ValidateLifetime = true,
                ValidateTokenReplay = false,
                ClockSkew = TimeSpan.Zero
            };
        }

        private static void SetOAuthClient(AuthenticationBuilder authenticationBuilder, IConfiguration configuration)
        {
            var configs = new List<OAuthConfiguration>();
            configuration.GetSection(nameof(OAuthConfiguration)).Bind(configs);

            //authenticationBuilder.Services.AddSingleton(configs);
            if (configs.Count > 0)
            {
                authenticationBuilder.Services.AddSingleton(configs[0]);
            }
            else
            {
                authenticationBuilder.Services.AddSingleton(new OAuthConfiguration());
            }

            foreach (var config in configs)
            {
                switch (config.Scheme)
                {
                    case QQDefaults.AuthenticationScheme:
                        authenticationBuilder
                            .AddQQ(options =>
                            {
                                options.ClientId = config.ClientId;
                                options.ClientSecret = config.ClientSecret;
                                if (!config.CallbackPath.IsNullOrWhiteSpace())
                                {
                                    options.CallbackPath = config.CallbackPath;
                                }
                            });
                        break;
                    case GithubDefaults.AuthenticationScheme:
                        authenticationBuilder
                            .AddGithub(options =>
                            {
                                options.ClientId = config.ClientId;
                                options.ClientSecret = config.ClientSecret;
                                if (!config.CallbackPath.IsNullOrWhiteSpace())
                                {
                                    options.CallbackPath = config.CallbackPath;
                                }
                            });
                        break;
                    default:
                        authenticationBuilder
                            .AddOpenIdConnect(config.Scheme, options =>
                            {
                                options.RequireHttpsMetadata = config.Authority.StartsWithIgnoreCase("https");
                                options.Authority = config.Authority;
                                options.ClientId = config.ClientId;
                                options.ClientSecret = config.ClientSecret;
                                options.ResponseType = config.ResponseType;
                                options.SaveTokens = true;

                                options.CallbackPath = config.CallbackPath;
                                options.SignInScheme = OAuthSignInAuthenticationDefaults.AuthenticationScheme;

                                foreach (var item in config.Scopes)
                                {
                                    options.Scope.Add(item);
                                }
                            });
                        break;
                }
            }
        }
    }
}
