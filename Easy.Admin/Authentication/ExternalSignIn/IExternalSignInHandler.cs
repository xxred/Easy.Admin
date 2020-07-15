using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Easy.Admin.Authentication.ExternalSignIn
{
    /// <summary>
    /// 移动端第三方登录处理
    /// </summary>
    public interface IExternalSignInHandler
    {
        /// <summary>
        /// 获取第三方提供者名称
        /// </summary>
        /// <returns></returns>
        public bool CheckName(string name);

        /// <summary>
        /// 处理器
        /// </summary>
        /// <returns></returns>
        public Task Handle(ExternalSignInContext externalSignInContext);
    }
}
