using Easy.Admin.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
namespace Easy.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 用户
    /// </summary>
    /// <typeparam name="User"></typeparam>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : EntityController<User>
    {
    }
}