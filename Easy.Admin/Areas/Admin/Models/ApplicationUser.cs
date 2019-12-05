using XCode.Membership;

namespace Easy.Admin.Areas.Admin.Models
{
    /// <summary>
    /// 用户
    /// </summary>
    public class ApplicationUser : User<ApplicationUser>
    {
        public override string RoleName => base.RoleName;
    }
}