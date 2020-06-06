namespace Easy.Admin.Services
{
    /// <summary>
    /// 发送邮件信息
    /// </summary>
    public interface IMailMessage
    {
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="key">邮箱</param>
        /// <param name="message">信息 </param>
        /// <returns></returns>
        void Send(string key, string message);
    }
}
