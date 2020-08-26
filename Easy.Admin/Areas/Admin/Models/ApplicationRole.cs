using System.Collections.Generic;
using NewLife.Reflection;
using XCode.Membership;

namespace Easy.Admin.Areas.Admin.Models
{
    /// <summary>
    /// 角色
    /// </summary>
    public class ApplicationRole : Role<ApplicationRole>
    {
        /// <summary>本角色权限集合</summary>
        public new IDictionary<int, PermissionFlags> Permissions => base.Permissions;

        /// <summary>当前角色拥有的资源</summary>
        public new int[] Resources => base.Resources;

        /// <summary>初始化时执行必要的权限检查，以防万一管理员无法操作</summary>
        private static void CheckRole()
        {
            typeof(Role<ApplicationRole>).Invoke("CheckRole");
        }
    }
}