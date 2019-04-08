using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NewLife.Data;
using Easy.Admin.Entities;

namespace Easy.Admin.Filters
{
    /// <summary>
    /// 结果过滤器，统一封装返回结果
    /// </summary>
    public class ApiResultFilterAttribute : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (!(context.Result is ObjectResult objectResult)) return;

            var data = objectResult.Value;
            var isApiResult = data is ApiResult;

            if (isApiResult) return;

            // 如果不是ApiResult，封装成ApiResult

            // 参数PageParameter暂时不提供，需要翻页参数的要自行指定
            //var p = context.ModelState.Values.FirstOrDefault(f =>
            //    false);

            var result = new ApiResult<object> { Data = data };

            //if (p != null && p is PageParameter page)
            //{
            //    result.Paper = page;
            //}

            context.Result = new ObjectResult(result);
        }
    }
}