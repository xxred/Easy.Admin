using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Easy.Admin.Authentication
{
    public class JwtBearerAuthenticationDefaults
    {
        /// <summary>
        /// The default value used for JwtBearerAuthenticationOptions.AuthenticationScheme
        /// </summary>
        public const string AuthenticationScheme = "JwtBearerSignIn";
        
        /// <summary>
        /// 用于签名的key
        /// </summary>
        public const string BearerSecretKey = "EasyAdminEasyAdminEasyAdmin";

        public const string ValidIssuer = "EasyAdminUser";

        public const string ValidAudience = "EasyAdminAudience";
    }
}
