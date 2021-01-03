using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Easy.Admin.Entities;
using Microsoft.AspNetCore.Http;

namespace Easy.Admin.Common
{
    /// <summary>
    /// 响应状态码重新定义
    /// </summary>
    public class ResponseStatusCode
    {
        /// <summary>
        /// 响应状态码重新定义入口
        /// </summary>
        internal static void SetResponseStatusCode(ApiResult result, HttpResponse response)
        {
            if (SetStatusCodeAndHttpStatusCode != null)
            {
                SetStatusCodeAndHttpStatusCode.Invoke(result, response);
                return;
            }

            result.Status = GetStatusCode(result.Status);

            if (SetHttpStatusCode)
            {
                response.StatusCode = result.Status;
            }
        }

        /// <summary>
        /// 是否设置Http状态码
        /// </summary>
        /// <remarks>
        /// 当值为true，响应结果的状态码同时设置到Http响应状态码。
        /// 比如响应结果的状态码为401，Http响应状态码也为401
        /// </remarks>
        public static bool SetHttpStatusCode = false;

        /// <summary>
        /// 获取状态码
        /// </summary>
        /// <remarks>
        /// 获取重新定义后的状态码，如果没有重新定义，返回传参的值。
        /// 比如203为未登录，需要重新定义成401，修改<see cref="StatusCode"/>即可
        /// </remarks>
        public static int GetStatusCode(int status)
        {
            if (StatusCode == null ||
                !StatusCode.TryGetValue(status, out var code)) return status;

            return code;
        }

        /// <summary>
        /// 状态码映射，将key对应状态码换成value对应状态码
        /// </summary>
        public static Dictionary<int, int> StatusCode;

        /// <summary>
        /// 如果有复杂的重新定义需求，全部自定义，自行设置结果状态码以及Http响应状态码
        /// </summary>
        public static Action<ApiResult, HttpResponse> SetStatusCodeAndHttpStatusCode;
    }
}
