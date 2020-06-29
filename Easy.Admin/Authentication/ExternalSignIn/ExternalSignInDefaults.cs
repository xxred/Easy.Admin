using Easy.Admin.Authentication.OAuthSignIn;

namespace Easy.Admin.Authentication.ExternalSignIn
{
    public class ExternalSignInDefaults : OAuthSignInAuthenticationDefaults
    {
        /// <summary>
        /// The default value used for JwtBearerAuthenticationOptions.AuthenticationScheme
        /// </summary>
        public new const string AuthenticationScheme = "ExternalSignIn";
    }
}
