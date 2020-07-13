using System.ComponentModel;
using Easy.Admin.Localization.Models;
using Microsoft.AspNetCore.Mvc;

namespace Easy.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 多语言记录
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("多语言记录")]
    public class LocalizationRecordsController : EntityController<LocalizationRecords>
    {
    }
}