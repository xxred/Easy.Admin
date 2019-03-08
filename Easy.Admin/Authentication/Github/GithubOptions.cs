using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Easy.Admin.Authentication.Github
{
    public class GithubOptions : OAuthOptions
    {
        public GithubOptions()
        {
            CallbackPath = new PathString("/sign-github");
            AuthorizationEndpoint = GithubDefaults.AuthorizationEndpoint;
            TokenEndpoint = GithubDefaults.TokenEndpoint;
            UserInformationEndpoint = GithubDefaults.UserInformationEndpoint;

            Scope.Add("openid");

            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier,"id");
            ClaimActions.MapJsonKey(ClaimTypes.Name, "name");

        }
    }
}
