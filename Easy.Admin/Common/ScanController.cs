using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.Controllers;
using Easy.Admin.Filters;
using Microsoft.AspNetCore.Authorization;
using NewLife;
using NewLife.Log;
using XCode;
using XCode.Membership;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ScanControllerExtensions
    {
        /// <summary>
        /// 扫描控制器，生成菜单
        /// </summary>
        /// <param name="servers"></param>
        /// <param name="configureScanControllerOptions"></param>
        /// <returns></returns>
        public static IServiceCollection ScanController(this IServiceCollection servers, Action<ScanControllerOptions> configureScanControllerOptions = null)
        {
            var scanControllerOptions = new ScanControllerOptions();
            if (configureScanControllerOptions != null)
            {
                configureScanControllerOptions(scanControllerOptions);
            }
            else
            {
                var baseControllerType = typeof(AdminControllerBase);

                scanControllerOptions.Namespace = baseControllerType.Namespace;
                scanControllerOptions.DisplayName = "系统";
                scanControllerOptions.RootUrl = "Admin";
                scanControllerOptions.ScanAssembly = baseControllerType.Assembly;
            }

            // 自动检查并添加菜单
            Task.Run(() =>
            {
                try
                {
                    ScanController(scanControllerOptions);
                }
                catch (Exception ex)
                {
                    XTrace.WriteException(ex);
                }
            });
            return servers;
        }

        /// <summary>自动扫描控制器，并添加到菜单</summary>
        /// <remarks>默认操作当前注册区域的下一级Controllers命名空间</remarks>
        private static void ScanController(ScanControllerOptions scanControllerOptions)
        {
            var displayName = scanControllerOptions.DisplayName;
            var description = scanControllerOptions.Description;
            var rootUrl = scanControllerOptions.RootUrl;
            var assembly = scanControllerOptions.ScanAssembly;
            var ns = scanControllerOptions.Namespace;

            var mf = ManageProvider.Menu;
            if (mf == null) return;

            using (var tran = (mf as IEntityFactory)?.CreateTrans())
            {
                XTrace.WriteLine("初始化[{0}]的菜单体系", rootUrl);
                var ms = mf.ScanController(rootUrl, assembly, ns);

                // 用于普通控制器，不设顶级Url
                //var root = mf.FindByFullName(ns);
                //if (root != null)
                //{
                //    root.Url = "~";
                //    (root as IEntity)?.Save();
                //}

                // 更新顶级名称为友好中文名
                var menu = mf.Root.FindByPath(rootUrl);
                if (menu != null && menu.DisplayName.IsNullOrEmpty())
                {
                    if (!displayName.IsNullOrEmpty()) menu.DisplayName = displayName;
                    if (!description.IsNullOrEmpty()) menu.Remark = description;

                    (menu as IEntity)?.Save();
                }

                // 遍历菜单，设置权限项
                foreach (var controller in ms)
                {
                    if (controller.FullName.IsNullOrEmpty()) continue;

                    var ctype = assembly.GetType(controller.FullName);
                    //ctype = controller.FullName.GetTypeEx(false);
                    if (ctype == null) continue;

                    // 添加该类型下的所有Action
                    var dic = new Dictionary<MethodInfo, Int32>();
                    foreach (var method in ctype.GetMethods())
                    {
                        if (method.IsStatic || !method.IsPublic) continue;
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

                    //controller.Url = "~/" + ctype.Name.TrimEnd("Controller");// 普通控制器，重置Url

                    (controller as IEntity)?.Save();
                }

                tran?.Commit();
            }
        }

        public class ScanControllerOptions
        {
            /// <summary>
            /// 顶级菜单名称
            /// </summary>
            public string DisplayName { get; set; }

            /// <summary>
            /// 顶级菜单描述
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// 顶级菜单Url
            /// </summary>
            public string RootUrl { get; set; }

            /// <summary>
            /// 待扫描的Assembly
            /// </summary>
            public Assembly ScanAssembly { get; set; }

            /// <summary>
            /// 控制器所在命名空间
            /// </summary>
            public string Namespace { get; set; }
        }
    }
}