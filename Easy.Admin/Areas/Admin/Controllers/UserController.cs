
using System;
using System.ComponentModel;
using Easy.Admin.Entities;
using Easy.Admin.Filters;
#if DEBUG
using Easy.Admin.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using XCode.Membership;

namespace Easy.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 用户
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("用户")]
    public class UserController : EntityController<ApplicationUser>
    {
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthorizeFilter(PermissionFlags.Insert)]
        [DisplayName("添加")]
        public override ApiResult<string> Post(ApplicationUser value)
        {
            // 新增账号默认密码123456
            value.Password = "123456".MD5();
            return base.Post(value);
        }
    }
}
#endif
