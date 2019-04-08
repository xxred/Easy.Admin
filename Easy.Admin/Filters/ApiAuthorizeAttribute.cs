// using System;
// using System.Net;
// using System.Net.Http;
// using System.Reflection;
// using System.Security.Principal;
// using System.Text;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Controllers;
// using Microsoft.AspNetCore.Mvc.Filters;
// using Microsoft.Net.Http.Headers;
// using NewLife.Reflection;
// using NewLife.Serialization;
// using Winoble.Entity;
// using Easy.Admin.Entities;

// namespace Easy.Admin.Filters
// {
//     /// <summary>
//     /// 验证特性
//     /// </summary>
//     public class ApiAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
//     {
//         /// <summary>
//         /// 重载验证
//         /// </summary>
//         /// <param name="context"></param>
//         public Task OnAuthorizationAsync(AuthorizationFilterContext context)
//         {
//             var ctrl = (ControllerActionDescriptor)context.ActionDescriptor;

//             //匿名不需要验证
//             var allowAnonymous = ctrl.MethodInfo.IsDefined(typeof(AllowAnonymousAttribute)) ||
//                       ctrl.ControllerTypeInfo.IsDefined(typeof(AllowAnonymousAttribute));

//             if (allowAnonymous) return Task.CompletedTask;

//             // 判断当前登录用户
//             var user = SetPrincipal(context);
//             if (!user)
//             {
//                 var content = new ApiResult<String>
//                 {
//                     Status = 203,
//                     Msg = "服务端拒绝访问：你没有权限，或者掉线了"
//                 };

//                 // 此处不能直接设置Response，要设置Result，后续过滤器才不会往下执行，下游判断Result不为空，直接执行结果，自动写入响应
//                 // 否则此处设置响应流，请求到达控制器，又会执行控制器的结果，因再次写入Response而抛异常
//                 context.Result = new ObjectResult(content);
//             }

//             return Task.CompletedTask;
//         }

//         Boolean SetPrincipal(AuthorizationFilterContext actionContext)
//         {
//             //获取浏览器的token
//             var token = actionContext.HttpContext.Request.Headers["Authorization"];
//             if (token.Count < 1)
//                 return false;
//             //从数据库里拿token 验证宿主身份
//             var ac = AccessToken.GetAccessToken(token.ToString());

//             //将搜索到的AccessToken映射到IIdentity，用户名，权限role
//             var iid = ac as IIdentity;
//             if (iid == null) return false;

//             //var api = actionContext.HttpContext.ControllerContext.Controller as BaseController;
//             //if (api == null)
//             //{
//             //    return false;
//             //}

//             // 角色列表
//             var up = new GenericPrincipal(iid, new string[] { iid.AuthenticationType });


//             actionContext.HttpContext.User = up;

//             Thread.CurrentPrincipal = up;

//             return true;
//         }
//     }
// }