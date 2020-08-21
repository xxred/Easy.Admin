using Microsoft.AspNetCore.Authentication;

namespace Easy.Admin.Authentication.IAM
{
    /// <inheritdoc />
    public class IAMOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// 是否开启IAM
        /// </summary>
        public bool UseIAM { get; set; } = false;

        /// <summary>
        /// IAM服务地址
        /// </summary>
        public string Url { get; set; }
    }
}
