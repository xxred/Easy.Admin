namespace Easy.Admin.Authentication.QQ
{
    /// <summary>
    /// https://wiki.connect.qq.com/%E4%BD%BF%E7%94%A8authorization_code%E8%8E%B7%E5%8F%96access_token
    /// </summary>
    public static class QQDefaults
    {
        public const string AuthenticationScheme = "QQ";

        public static readonly string DisplayName = "QQ";

        public static readonly string AuthorizationEndpoint = "https://graph.qq.com/oauth2.0/authorize";

        public static readonly string TokenEndpoint = "https://graph.qq.com/oauth2.0/token";

        public static readonly string OpenIdEndpoint = "https://graph.qq.com/oauth2.0/me";

        public static readonly string UserInformationEndpoint = "https://graph.qq.com/user/get_user_info";

    }
}
