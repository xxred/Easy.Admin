#if DEBUG
using Easy.Admin.Entities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;

namespace Easy.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 用户
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("测试")]
    public class TestController : AdminControllerBase
    {
        /// <summary>
        /// 测试上传文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public ApiResult TestUploadFile(IFormCollection formCollection)
        {
            var res = base.UploadFile("Test");
            return res;
        }
    }
}
#endif