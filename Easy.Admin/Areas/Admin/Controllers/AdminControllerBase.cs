using System;
using System.Collections.Generic;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Common;
using Easy.Admin.Entities;
using Easy.Admin.Filters;
using Easy.Admin.Localization.Resources;
using Easy.Admin.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NewLife.Data;
using XCode.Membership;

namespace Easy.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 基类Api
    /// </summary>
    [Route("api/[controller]")]
    [ApiResultFilter]
    [ApiController]
    [ApiAuthenticateFilter()]
    [EnableCors]
    public class AdminControllerBase : ControllerBase
    {
        /// <summary>
        /// 用于请求的语言定位器
        /// </summary>
        protected IStringLocalizer<Request> RequestLocalizer => HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Request>>();

        private IUser _appUser;
        /// <summary>
        /// 当前用户
        /// </summary>
        protected IUser AppUser
        {
            get => _appUser ??= HttpContext.Features.Get<IUser>();
            set => _appUser = value;
        }

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        protected bool IsSupperAdmin => AppUser?.Role != null && AppUser.Role.IsSystem;

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

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="keyPrefix"></param>
        /// <returns></returns>
        protected virtual ApiResult<List<string>> UploadFile(string keyPrefix)
        {
            if (!Request.HasFormContentType)
            {
                throw ApiException.Common(RequestLocalizer["There is no form content"]);
            }

            var files = Request.Form.Files;

            if (files.Count < 1)
            {
                throw ApiException.Common(RequestLocalizer["The uploaded file was not found"]);
            }

            var keyBase = $"{keyPrefix ?? ""}/{DateTime.Now:yyyy/MM/dd}/".TrimStart('/');
            var urlList = new List<string>();
            var fileUpload = (IFileUpload)HttpContext.RequestServices.GetService(typeof(IFileUpload));

            foreach (var file in files)
            {
                var ext = System.IO.Path.GetExtension(file.FileName);
                var content = file.OpenReadStream();

                var fileName = content.ToMD5() + ext;
                var key = keyBase + fileName;

                var picUrl = fileUpload.PutObject(key, content);

                urlList.Add(picUrl);
            }

            return ApiResult.Ok(urlList);
        }
    }
}