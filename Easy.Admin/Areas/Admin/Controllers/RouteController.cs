using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using XCode.Membership;

namespace Easy.Admin.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("前端路由")]
    public class RouteController : AdminControllerBase
    {
        /// <summary>
        /// 获取前端路由
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var menus = Menu.Meta.Cache.Entities.Where(w => w.Visible).ToList();

            var list = new List<object>();

            var prefix = "/";

            var pList = menus.Where(w => w.ParentID == 0).ToList();

            var user = HttpContext.User.Identity as IUser;

            foreach (var item in pList)
            {
                if (!item.Permission.IsNullOrWhiteSpace() && !user.Has(item, PermissionFlags.Detail)) continue;

                var route = new
                {
                    path = prefix + item.Name,
                    component = "views/layout/Layout",
                    name = item.Name,
                    meta = new
                    {
                        title = item.DisplayName,
                        icon = item.Icon ?? "international"
                    },
                    // 子菜单权限项为空，或者不为空且有权限访问
                    children = menus.Where(w => w.ParentID == item.ID &&
                                                ((!w.Permission.IsNullOrWhiteSpace() && user.Has(w, PermissionFlags.Detail))
                                                 || w.Permission.IsNullOrWhiteSpace())
                                                ).Select(ctrl =>
                                                {
                                                    var name = ctrl.Name;
                                                    var title = ctrl.DisplayName;
                                                    var route1 = new
                                                    {
                                                        path = prefix + name + "/Index",
                                                        name = name,
                                                        //template = $"<table-base table-name=\"{name}\" />",
                                                        meta = new
                                                        {
                                                            title = title,
                                                            icon = ctrl.Icon ?? "international"
                                                        }
                                                    };
                                                    return route1;
                                                }).ToList()
                };

                // 没有子菜单，并且本身并不是控制器，不添加
                if (route.children.Count >= 1 || item.FullName.EndsWith("Controller"))
                {
                    list.Add(route);
                }
            }

            return new JsonResult(list);
        }
    }
}