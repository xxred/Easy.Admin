using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Easy.Admin.Authentication
{
    public class JwtBearerAuthenticationOptions : JwtBearerOptions
    {
        /// <summary>
        /// token过期时间，默认一个小时
        /// </summary>
        public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.FromMinutes(60);
    }
}
