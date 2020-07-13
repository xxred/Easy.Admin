using System.ComponentModel;
using Easy.Admin.Localization.Models;
using Microsoft.AspNetCore.Mvc;

namespace Easy.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 语言
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("语言")]
    public class LanguagesController : EntityController<Languages>
    {
    }
}