﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Easy.Admin.Identity.IAM
{
    public class IAMOptions
    {
        public bool UseIAM { get; set; } = false;

        /// <summary>
        /// IAM服务地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 代理链接列表
        /// </summary>
        public IList<string> ProxyUrlList { get; set; }
    }
}
