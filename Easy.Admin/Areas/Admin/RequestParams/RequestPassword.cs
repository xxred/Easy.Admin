using NewLife.Reflection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using XCode.Membership;

namespace Easy.Admin.Areas.Admin.RequestParams
{
    public class RequestPassword
    {
        /// <summary>
        /// 旧密码
        /// </summary>
        public string OldPassword { get; set; }

        /// <summary>
        /// 新密码
        /// </summary>
        public string NewPassword { get; set; }
        
        /// <summary>
        /// 验证码
        /// </summary>
        public string VerCode { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Mail { get; set; }

        /// <summary>
        /// 国际区号，用于手机
        /// </summary>
        public string InternationalAreaCode { get; set; }

        /// <summary>
        /// 0-旧密码，1-手机验证码，2-邮箱验证码
        /// </summary>
        public int Type { get; set; }
    }
}
