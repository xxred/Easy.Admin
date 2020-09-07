using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using NewLife.Model;
using XCode.Membership;

namespace Easy.Admin.Services
{
    public interface IUserService
    {
        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="names">字段名</param>
        /// <param name="values">对应值</param>
        /// <returns></returns>
        Task<IdentityResult> CreateAsync(string[] names, object[] values);

        /// <summary>
        /// 根据用户名密码创建用户
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<IdentityResult> CreateAsync(string userName, string password);

        /// <summary>
        /// 根据右键查找用户
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<IUser> FindByEmailAsync(string email);

        /// <summary>
        /// 根据id查找用户
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IUser> FindByIdAsync(string userId);

        /// <summary>
        /// 第三方登录查找
        /// </summary>
        /// <param name="loginProvider">第三方提供者</param>
        /// <param name="providerKey">token</param>
        /// <returns></returns>
        Task<IUser> FindByLoginAsync(string loginProvider, string providerKey);

        /// <summary>
        /// 根据用户名查找
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<IUser> FindByNameAsync(string name);

        /// <summary>
        /// 根据手机号查找
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        Task<IUser> FindByPhoneNumberAsync(string phoneNumber);

        /// <summary>
        /// 查找或创建用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="properties"></param>
        /// <param name="createUserOnOAuthLogin"></param>
        /// <returns></returns>
        Task<IManageUser> GetOrCreateUserAsync(ClaimsPrincipal user, AuthenticationProperties properties,
            bool createUserOnOAuthLogin);

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="dic">字典中key为字段名，value为值</param>
        /// <returns></returns>
        Task UpdateAsync(IDictionary<string, object> dic);

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="names">字段名</param>
        /// <param name="values">对应值</param>
        /// <returns></returns>
        Task UpdateAsync(string[] names, object[] values);

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="rememberMe"></param>
        /// <returns></returns>
        Task<SignInResult> LoginAsync(string username, string password, bool rememberMe = false);

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="rememberMe"></param>
        /// <returns></returns>
        Task<SignInResult> LoginAsync(IUser user, string password, bool rememberMe = false);

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<SignInResult> LoginAsync(IUser user);

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        Task SignOutAsync();

        /// <summary>
        /// 删除账号资料
        /// </summary>
        /// <returns></returns>
        Task DeleteAccountAsync(IUser user);
    }
}
