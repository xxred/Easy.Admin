using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Easy.Admin.Authentication.JwtBearer
{
    public class JwtBearerAuthenticationOptions : JwtBearerOptions
    {
        /// <summary>
        /// token过期时间，默认一个小时
        /// </summary>
        public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.FromMinutes(60);

        /// <summary>
        /// Cookie中记录token的key
        /// </summary>
        public string TokenKey { get; set; } = "Admin-Token";

        /// <summary>
        /// Cookie中记录returnUrl的key
        /// </summary>
        public string ReturnUrlKey { get; set; } = "returnUrl";

        /// <summary>
        /// 第三方登录后是否自动创建用户
        /// </summary>
        public bool CreateUserOnOAuthLogin { get; set; } = true;
    }
}
