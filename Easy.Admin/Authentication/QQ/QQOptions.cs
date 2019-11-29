using Microsoft.AspNetCore.Authentication;
using Easy.Admin.Authentication.OAuthSignIn;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace Easy.Admin.Authentication.QQ
{
    /// <summary>
    /// Configuration options for <see cref="QQHandler"/>.
    /// </summary>
    public class QQOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new <see cref="QQOptions"/>.
        /// </summary>
        public QQOptions()
        {
            CallbackPath = new PathString("/sign-qq");
            AuthorizationEndpoint = QQDefaults.AuthorizationEndpoint;
            TokenEndpoint = QQDefaults.TokenEndpoint;
            OpenIdEndpoint = QQDefaults.OpenIdEndpoint;
            UserInformationEndpoint = QQDefaults.UserInformationEndpoint;

            Scope.Add("get_user_info");

            ClaimActions.MapJsonKey(OAuthSignInAuthenticationDefaults.Sub, "openid");
            ClaimActions.MapJsonKey(OAuthSignInAuthenticationDefaults.GivenName, "nickname");
            ClaimActions.MapJsonKey(OAuthSignInAuthenticationDefaults.Gender, "gender");
            ClaimActions.MapJsonKey(OAuthSignInAuthenticationDefaults.Avatar, "figureurl_2");

            SignInScheme = OAuthSignInAuthenticationDefaults.AuthenticationScheme;
        }

        /// <summary>
        /// 获取OpenId的链接
        /// </summary>
        public string OpenIdEndpoint { get; set; }

    }
}
