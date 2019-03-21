using Microsoft.AspNetCore.Authentication;
using System;
using Easy.Admin.Authentication.Github;

namespace Microsoft.Extensions.DependencyInjection
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
