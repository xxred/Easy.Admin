using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Easy.Admin.Areas.Admin.ResponseParams;
using Easy.Admin.Entities;
using Easy.Admin.Filters;
using Microsoft.AspNetCore.Mvc;
using XCode.Membership;

namespace Easy.Admin.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("菜单")]
    public class MenuController : EntityController<Menu>
    {
        /// <summary>
        /// 获取所有菜单
        /// </summary>
        ///// <returns></returns>
        [HttpGet("[action]")]
        [ApiAuthorizeFilter(PermissionFlags.Detail)]
        public ApiResult<List<ResponseMenu>> GetAllMenu()
        {
            var list = Menu.Root.Childs.Select(s=> new ResponseMenu(s)).ToList();
            return ApiResult.Ok(list);
        }

        ///// <summary>
        ///// 修改
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //[ApiAuthorizeFilter(PermissionFlags.Update)]
        //public override ApiResult Put(Menu value)
        //{
        //    return base.Put(value);
        //}
    }
}