#if DEBUG
using Easy.Admin.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using XCode.Membership;

namespace Easy.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 用户
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : EntityController<UserX>
    {
    }
}
#endif
