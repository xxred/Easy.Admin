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
        public Int32 Status { get; set; }
        public override String Message { get; }
        /// <summary>
        /// api异常
        /// </summary>
        /// <param name="status">状态码</param>
        /// <param name="msg">异常消息</param>
        public ApiException(Int32 status, String msg)
        {
            Status = status;
            Message = msg;
        }

        /// <summary>
        /// 异常通用封装。状态402，信息显示给用户看
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ApiException Common(string msg)
        {
            return new ApiException(402, msg);
        }
    }
}