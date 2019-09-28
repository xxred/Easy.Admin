using System;
using NewLife.Data;

namespace Easy.Admin.Entities
{
    /// <summary>
    /// 带返回类型的结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T> : ApiResult
    {
        /// <summary>数据</summary>
        public T Data { get; set; }

        /// <summary>
        /// 返回带Data的正常响应结果
        /// </summary>
        /// <param name="data"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static ApiResult<T> Ok(T data, PageParameter p = null)
        {
            return new ApiResult<T> { Data = data, Paper = p };
        }
    }

    /// <summary>Ajax返回结果</summary>
    public class ApiResult
    {
        /// <summary>状态 0表示成功,大于0表示失败</summary>
        public Int32 Status { get; set; } = 0;

        /// <summary>信息</summary>
        public String Msg { get; set; } = "ok";

        /// <summary>
        /// 分页信息
        /// </summary>
        public PageParameter Paper { get; set; }

        /// <summary>
        /// 返回错误结果的包装
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static ApiResult Err(String msg = null, Int32 status = 402)
        {
            return new ApiResult { Status = status, Msg = msg };
        }

        /// <summary>
        /// 返回带Data的正常响应结果
        /// </summary>
        /// <param name="data"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static ApiResult<T> Ok<T>(T data, PageParameter p = null)
        {
            return new ApiResult<T> { Data = data, Paper = p };
        }

        /// <summary>
        /// 返回带Data的正常响应结果
        /// </summary>
        /// <returns></returns>
        public static ApiResult<string> Ok()
        {
            return ApiResult.Ok((string)null);
        }
    }
}