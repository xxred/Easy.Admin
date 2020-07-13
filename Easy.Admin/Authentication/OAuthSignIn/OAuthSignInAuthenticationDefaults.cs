using System.IdentityModel.Tokens.Jwt;

namespace Easy.Admin.Authentication.OAuthSignIn
{
    public class OAuthSignInAuthenticationDefaults
    {
        /// <summary>
        /// The default value used for JwtBearerAuthenticationOptions.AuthenticationScheme
        /// </summary>
        public const string AuthenticationScheme = "OAuthSignIn";

        /// <summary>
        /// 唯一标识
        /// </summary>
        public const string Sub = JwtRegisteredClaimNames.Sub;

        /// <summary>
        /// 唯一名称、用户名
        /// </summary>
        public const string UniqueName = JwtRegisteredClaimNames.UniqueName;

        /// <summary>
        /// 昵称，用户名称
        /// </summary>
        public const string GivenName = JwtRegisteredClaimNames.GivenName;

        /// <summary>
        /// 性别
        /// </summary>
        public const string Gender = JwtRegisteredClaimNames.Gender;

        /// <summary>
        /// 头像地址
        /// </summary>
        public const string Avatar = "avatar";

        /// <summary>
        /// 语言
        /// </summary>
        public const string Lang = "lang";
    }
}
