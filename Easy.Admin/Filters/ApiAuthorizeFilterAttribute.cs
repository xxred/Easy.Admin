using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Easy.Admin.Common;
using Easy.Admin.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using NewLife;
using NewLife.Log;
using NewLife.Reflection;
using XCode;
using XCode.Membership;

namespace Easy.Admin.Filters
{
    /// <summary>实体授权特性</summary>
    public class ApiAuthorizeFilterAttribute : Attribute, IAuthorizationFilter
    {
        #region 属性
        /// <summary>授权项</summary>
        public PermissionFlags Permission { get; }

        /// <summary>是否全局特性</summary>
        internal Boolean IsGlobal;
        #endregion

        #region 构造
        static ApiAuthorizeFilterAttribute()
        {
            XTrace.WriteLine("注册过滤器：{0}", typeof(ApiAuthorizeFilterAttribute).FullName);
        }

        /// <summary>实例化实体授权特性</summary>
        public ApiAuthorizeFilterAttribute() { }

        /// <summary>实例化实体授权特性</summary>
        /// <param name="permission"></param>
        public ApiAuthorizeFilterAttribute(PermissionFlags permission)
        {
            if (permission <= PermissionFlags.None) throw new ArgumentNullException(nameof(permission));

            Permission = permission;
        }
        #endregion

        #region 方法
        /// <summary>授权发生时触发</summary>
        /// <param name="filterContext"></param>
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            /*
             * 验证范围：
             * 1，魔方区域下的所有控制器
             * 2，所有带有EntityAuthorize特性的控制器或动作
             */
            var act = filterContext.ActionDescriptor;
            var ctrl = (ControllerActionDescriptor)act;

            // 允许匿名访问时，直接跳过检查
            if (
                ctrl.MethodInfo.IsDefined(typeof(AllowAnonymousAttribute)) ||
                ctrl.ControllerTypeInfo.IsDefined(typeof(AllowAnonymousAttribute))) return;

            // 如果控制器或者Action放有该特性，则跳过全局
            var hasAtt =
                ctrl.MethodInfo.IsDefined(typeof(ApiAuthorizeFilterAttribute), true) ||
                ctrl.ControllerTypeInfo.IsDefined(typeof(ApiAuthorizeFilterAttribute));

            if (IsGlobal && hasAtt) return;

            // 根据控制器定位资源菜单
            var menu = GetMenu(filterContext);

            // 如果已经处理过，就不处理了
            if (filterContext.Result != null) return;

            if (!AuthorizeCore(filterContext.HttpContext))
            {
                HandleUnauthorizedRequest(filterContext);
            }
        }

        /// <summary>授权核心</summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected bool AuthorizeCore(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var ctx = httpContext;
            var user = ctx.User.Identity as IUser;
            if (user == null)
            {
                return false;
            }

            // 判断权限
            if (!(ctx.Items["CurrentMenu"] is IMenu menu) || !(user is IUser user2)) return false;
            return user2.Has(menu, Permission);
        }

        /// <summary>无权限请求</summary>
        /// <param name="filterContext"></param>
        protected void HandleUnauthorizedRequest(AuthorizationFilterContext filterContext)
        {
            var content = new ApiResult<String>
            {
                Msg = "No permission" // 没有权限
            };

            //content.Status = ResponseStatusCode.GetStatusCode(401);

            //if (ResponseStatusCode.SetHttpStatusCode)
            //{
            //    filterContext.HttpContext.Response.StatusCode = content.Status;
            //}

            ResponseStatusCode.SetResponseStatusCode(content, filterContext.HttpContext.Response);

            // 此处不能直接设置Response，要设置Result，后续过滤器才不会往下执行，下游判断Result不为空，直接执行结果，自动写入响应
            // 否则此处设置响应流，请求到达控制器，又会执行控制器的结果，因再次写入Response而抛异常
            filterContext.Result = new ObjectResult(content);


        }

        private IMenu GetMenu(AuthorizationFilterContext filterContext)
        {
            var act = (ControllerActionDescriptor)filterContext.ActionDescriptor;
            //var ctrl = act.ControllerDescriptor;
            var type = act.ControllerTypeInfo;
            var fullName = type.FullName + "." + act.ActionName;

            var ctx = filterContext.HttpContext;
            var mf = ManageProvider.Menu;
            var menu = ctx.Items["CurrentMenu"] as IMenu;
            if (menu == null)
            {
                menu = mf.FindByFullName(fullName) ?? mf.FindByFullName(type.FullName);

                // 当前菜单
                //filterContext.Controller.ViewBag.Menu = menu;
                // 兼容旧版本视图权限
                ctx.Items["CurrentMenu"] = menu;
            }

            if (menu == null) XTrace.WriteLine("设计错误！验证权限时无法找到[{0}/{1}]的菜单", type.FullName, act.ActionName);

            return menu;
        }

        private static ConcurrentDictionary<String, Type> _ss = new ConcurrentDictionary<String, Type>();

        private bool CreateMenu(Type type)
        {
            if (!_ss.TryAdd(type.Namespace, type)) return false;

            var mf = ManageProvider.Menu;
            var ms = mf.ScanController(type.Namespace.TrimEnd(".Controllers"), type.Assembly, type.Namespace);

            var root = mf.FindByFullName(type.Namespace);
            if (root != null)
            {
                root.Url = "~";
                (root as IEntity).Save();
            }

            // 遍历菜单，设置权限项
            foreach (var controller in ms)
            {
                if (controller.FullName.IsNullOrEmpty()) continue;

                var ctype = type.Assembly.GetType(controller.FullName);
                //ctype = controller.FullName.GetTypeEx(false);
                if (ctype == null) continue;

                // 添加该类型下的所有Action
                var dic = new Dictionary<MethodInfo, Int32>();
                foreach (var method in ctype.GetMethods())
                {
                    if (method.IsStatic || !method.IsPublic) continue;
                    if (!method.ReturnType.As<ActionResult>()) continue;
                    if (method.GetCustomAttribute<AllowAnonymousAttribute>() != null) continue;

                    var att = method.GetCustomAttribute<ApiAuthorizeFilterAttribute>();
                    if (att != null && att.Permission > PermissionFlags.None)
                    {
                        var dn = method.GetDisplayName();
                        var pmName = !dn.IsNullOrEmpty() ? dn : method.Name;
                        if (att.Permission <= PermissionFlags.Delete) pmName = att.Permission.GetDescription();
                        controller.Permissions[(Int32)att.Permission] = pmName;
                    }
                }

                controller.Url = "~/" + ctype.Name.TrimEnd("Controller");

                (controller as IEntity).Save();
            }

            return true;
        }
        #endregion 
    }
}
