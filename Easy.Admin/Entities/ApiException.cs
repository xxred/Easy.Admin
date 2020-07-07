using System;

namespace Easy.Admin.Entities
{
    /// <summary>
    /// 异常信息
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// 状态码0，为成功，大于0为失败
        /// </summary>
        public int Status { get; set; }

        public override string Message { get; }

        /// <summary>
        /// api异常
        /// </summary>
        /// <param name="status">状态码</param>
        /// <param name="msg">异常消息</param>
        public ApiException(int status, string msg)
        {
            Status = status;
            Message = msg;
        }

        /// <summary>
        /// 异常通用封装。状态402，信息显示给用户看
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static ApiException Common(string msg, int status = 402)
        {
            return new ApiException(status, msg);
        }
    }
}