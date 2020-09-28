using NewLife.Reflection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using XCode.Membership;

namespace Easy.Admin.Areas.Admin.RequestParams
{
    public class RequestUserInfo
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
        /// 用户ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public SexKinds? Sex { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Mail { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        public string Mobile { get; set; }

        private DateTime? _birthday;
        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday
        {
            get
            {
                var _1970 = new DateTime(1970, 01, 01);
                return _birthday == null || _birthday < _1970 ? _1970 : _birthday;
            }
            set => _birthday = value;
        }
    }
}
