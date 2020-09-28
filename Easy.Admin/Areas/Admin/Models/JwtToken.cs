using System;

namespace Easy.Admin.Areas.Admin.Models
{
    public class JwtToken
    {
        public JwtToken()
        {

        }

        public JwtToken(string token)
        {
            Token = token;
        }

        public string Token { get; set; }

        public DateTime? Expires { get; set; }

        public UserInfo UserInfo { get; set; }
    }

    public class UserInfo
    {

        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 0或空-未知，1-男，2-女
        /// </summary>
        public int? Sex { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; } = new DateTime(1970, 01, 01);
    }
}
