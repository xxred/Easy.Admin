using System;
using System.Collections.Generic;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Common;
using Easy.Admin.Entities;
using Easy.Admin.Filters;
using Easy.Admin.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
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

        private IUser _appUser;

        /// <summary>
        /// 当前用户
        /// </summary>
        public IUser AppUser
        {
            get => _appUser ?? (_appUser = HttpContext.Features.Get<IUser>());
            set => _appUser = value;
        }

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public bool? IsSupperAdmin => AppUser?.Role.IsSystem;


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
        protected virtual ApiResult UploadFile(string keyPrefix)
        {
            if (!Request.HasFormContentType)
            {
                return ApiResult.Err("没有表单内容");
            }

            var files = Request.Form.Files;

            if (files.Count < 1)
            {
                return ApiResult.Err("没有找到上传的文件");
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