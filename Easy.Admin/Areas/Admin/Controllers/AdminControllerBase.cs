using System;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Entities;
using Easy.Admin.Filters;
using Microsoft.AspNetCore.Mvc;
using NewLife.Data;

namespace Easy.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 基类Api
    /// </summary>
    [Route("api/[controller]")]
    [ApiResultFilter]
    [ApiController]
    //[ApiAuthenticateFilter()]
    public class AdminControllerBase : ControllerBase
    {

        private ApplicationUser _appUser;

        /// <summary>
        /// 当前用户
        /// </summary>
        public ApplicationUser AppUser
        {
            get => _appUser ?? (_appUser = HttpContext.Features.Get<ApplicationUser>());
            set => _appUser = value;
        }

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public bool IsSupperAdmin => AppUser.Role.IsSystem;


        /// <summary>
        /// 返回可带分页的结果
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="data"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        protected ApiResult Ok<TResult>(TResult data, PageParameter p = null)
        {
            return ApiResult.Ok(data, p);
        }

        /// <summary>
        /// 返回默认状态为402的结果
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        protected ApiResult Error(String msg = null, Int32 status = 402)
        {
            return ApiResult.Err(msg, status);
        }
    }
}