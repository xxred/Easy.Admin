namespace Easy.Admin.Services
{
    /// <summary>
    /// 发送手机信息
    /// </summary>
    public interface IPhoneMessage
    {
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="key">手机号，带上国际区号</param>
        /// <param name="message">信息 </param>
        /// <returns></returns>
        void Send(string key, string message);
    }
}
