﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Entities;
using Easy.Admin.Localization.Resources;
using Easy.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NewLife.Log;
using XCode.Membership;

namespace Easy.Admin.Filters
{
    /// <summary>
    /// 认证过滤器，处理用户信息
    /// </summary>
    public class ApiAuthenticateFilterAttribute : Attribute, IAsyncAuthorizationFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var requestLocalizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Request>>();

            var ctrl = (ControllerActionDescriptor)context.ActionDescriptor;

            //匿名不需要验证
            var allowAnonymous = ctrl.MethodInfo.IsDefined(typeof(AllowAnonymousAttribute)) ||
                      ctrl.ControllerTypeInfo.IsDefined(typeof(AllowAnonymousAttribute));

            if (allowAnonymous) return Task.CompletedTask;

            // 判断当前登录用户
            var user = SetPrincipal(context);
            if (!user)
            {
                var content = new ApiResult<string>
                {
                    Status = 203,
                    Msg = requestLocalizer["No login or login timeout"]
                };

                // 此处不能直接设置Response，要设置Result，后续过滤器才不会往下执行，下游判断Result不为空，直接执行结果，自动写入响应
                // 否则此处设置响应流，请求到达控制器，又会执行控制器的结果，因再次写入Response而抛异常
                context.Result = new ObjectResult(content);
            }

            return Task.CompletedTask;
        }

        bool SetPrincipal(AuthorizationFilterContext actionContext)
        {
            var context = actionContext.HttpContext;
            var requestLocalizer = context.RequestServices.GetRequiredService<IStringLocalizer<Request>>();

            var userService =
                (actionContext.HttpContext.RequestServices.GetRequiredService(typeof(IUserService)) as IUserService) ?? throw ApiException.Common(requestLocalizer["IUserService is not registered"], 500);

            if (!context.User.Identity.IsAuthenticated)
            {
                return false;
            }

            var id = context.User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
            if (id == null)
            {
                id = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            if (id == null)
            {
                id = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            }

            if (id == null)
            {
                XTrace.WriteLine("context.User中找不到存放id的声明");
                throw new ApiException(500, requestLocalizer["Could not find the id claim in context.User"]);
            }

            var user = userService.FindByIdAsync(id).Result;

            if (user == null)
            {
                return false;
            }

            var ac = user;

            ////将搜索到的AccessToken映射到IIdentity，用户名，权限role
            var iid = (IIdentity)ac;

            // 角色列表
            var up = new GenericPrincipal(iid, new[] { iid.AuthenticationType });

            context.Features[typeof(IUser)] = iid as IUser;
            context.User = up;

            Thread.CurrentPrincipal = up;

            return true;
        }
    }
}
