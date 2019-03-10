using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Easy.Admin.Authentication.Github
{
    public static class GithubExtensions
    {
        public static AuthenticationBuilder AddGithub(this AuthenticationBuilder builder)
            => builder.AddGithub(GithubDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddGithub(this AuthenticationBuilder builder,
            Action<GithubOptions> configureOptions)
            => builder.AddGithub(GithubDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddGithub(this AuthenticationBuilder builder,
            string authenticationScheme, Action<GithubOptions> configureOptions)
            => builder.AddGithub(authenticationScheme, GithubDefaults.DisplayName, configureOptions);


        public static AuthenticationBuilder AddGithub(this AuthenticationBuilder builder,
            string authenticationScheme, string displayName, Action<GithubOptions> configureOptions)
            => builder.AddOAuth<GithubOptions, GithubHandler>(authenticationScheme, displayName, configureOptions);
    }
}
