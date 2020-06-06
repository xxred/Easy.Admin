namespace Easy.Admin.Areas.Admin.RequestParams
{
    public class RequestRegister
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Mail { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        public string VerCode { get; set; }

        /// <summary>
        /// 0-用户名密码，1-手机密码，2-手机验证码，3-邮箱密码
        /// </summary>
        public int Type { get; set; }
    }
}
