using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Google;

namespace Easy.Admin.Authentication.Github
{
    public class GithubOptions : OAuthOptions
    {
        public GithubOptions()
        {
            // 请求地址配置
            CallbackPath = new PathString("/sign-github");
            AuthorizationEndpoint = GithubDefaults.AuthorizationEndpoint;
            TokenEndpoint = GithubDefaults.TokenEndpoint;
            UserInformationEndpoint = GithubDefaults.UserInformationEndpoint;

            // 授权范围配置
            Scope.Add("openid");

            // 从第三方出请求回来的用户信息对应关系
            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
            ClaimActions.MapJsonKey(GithubDefaults.AvatarClaimTypes, "avatar_url");

        }
    }
}
