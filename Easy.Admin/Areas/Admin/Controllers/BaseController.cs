using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NewLife.Data;
using NewLife.Remoting;
using Easy.Admin.Entities;
using Easy.Admin.Filters;

namespace Easy.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 基类Api
    /// </summary>
    [ApiResultFilter]
    public class BaseController : ControllerBase
    {
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