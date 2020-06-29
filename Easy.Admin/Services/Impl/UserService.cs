using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Authentication.OAuthSignIn;
using Easy.Admin.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using NewLife.Log;
using NewLife.Model;
using Swashbuckle.AspNetCore.SwaggerGen;
using XCode.Membership;

namespace Easy.Admin.Services.Impl
{
    public class UserService<TUser> : IUserService where TUser : User<TUser>, new()
    {
        private readonly UserManager<TUser> _userManager;
        private readonly SignInManager<TUser> _signInManager;

        public UserService(UserManager<TUser> userManager, SignInManager<TUser> signInManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        #region 查找或创建
        /// <summary>
        /// 创建用户并保存到数据库
        /// </summary>
        /// <param name="names"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public virtual async Task<IdentityResult> CreateAsync(string[] names, object[] values)
        {
            var user = new TUser();

            for (int i = 0; i < names.Length; i++)
            {
                user.SetItem(names[i], values[i]);
            }

            var result = await _userManager.CreateAsync(user, user.Password);
            return result;
        }

        /// <summary>
        /// 创建用户并保存到数据库
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IdentityResult> CreateAsync(string userName, string password)
        {
            var user = new TUser { Name = userName };

            var result = await _userManager.CreateAsync(user, password);
            return result;
        }

        public virtual async Task<IUser> FindByEmailAsync(string email) => await _userManager.FindByEmailAsync(email);

        public virtual async Task<IUser> FindByIdAsync(string userId) => await _userManager.FindByIdAsync(userId);

        public virtual async Task<IUser> FindByLoginAsync(string loginProvider, string providerKey)
            => await _userManager.FindByLoginAsync(loginProvider, providerKey);

        public virtual async Task<IUser> FindByNameAsync(string name) => await _userManager.FindByNameAsync(name);

        public virtual Task<IUser> FindByPhoneNumberAsync(string phoneNumber)
        {
            return Task.FromResult(User<TUser>.FindByMobile(phoneNumber) as IUser);
        }

        /// <summary>
        /// 获取或创建用户
        /// </summary>
        /// <returns></returns>
        public async Task<IManageUser> GetOrCreateUserAsync(ClaimsPrincipal user, AuthenticationProperties properties, bool createUserOnOAuthLogin)
        {
            var provider = properties.Items["scheme"];
            var openid = user.FindFirstValue(OAuthSignInAuthenticationDefaults.Sub);

            var uc = UserConnect.FindByProviderAndOpenID(provider, openid);

            IManageUser appUser;

            if (uc == null)
            {
                if (!createUserOnOAuthLogin)
                {
                    throw ApiException.Common("用户不存在，请联系管理员");
                }

                uc = new UserConnect() { Provider = provider, OpenID = openid, Enable = true };
                uc.Fill(user);

                appUser = new TUser
                {
                    Name = Guid.NewGuid().ToString().Substring(0, 8), 
                    Enable = true, 
                    RoleID = 4
                }; // 角色id 4 为游客

                // 通过第三方登录创建的用户设置随机密码
                var result = await _userManager.CreateAsync((TUser) appUser, Guid.NewGuid().ToString().Substring(0, 8));

                if (!result.Succeeded)
                {
                    throw ApiException.Common($"创建用户失败：{result.Errors.First().Description}");
                }

                uc.UserID = appUser.ID;
            }
            else
            {
                appUser = await _userManager.FindByIdAsync(uc.UserID.ToString()) as IManageUser;
            }

            if (appUser == null)
            {
                throw ApiException.Common($"找不到该用户");
            }

            if (!appUser.Enable)
            {
                throw ApiException.Common($"用户已被禁用");
            }

            // 填充用户信息
            Fill(appUser, user);

            if (appUser is IAuthUser user3)
            {
                user3.Logins++;
                user3.LastLogin = DateTime.Now;
                //user3.LastLoginIP = Request.Host.Host;
                user3.Save();
            }

            try
            {
                uc.Save();
            }
            catch (Exception ex)
            {
                //为了防止某些特殊数据导致的无法正常登录，把所有异常记录到日志当中。忽略错误
                XTrace.WriteException(ex);
            }

            return appUser;
        }

        /// <summary>
        /// 填充用户信息
        /// </summary>
        private void Fill(IManageUser user, ClaimsPrincipal user1)
        {
            if (user is IUser user2)
            {
                if (user2.Sex == SexKinds.未知 && user1.HasClaim(h => h.Type == OAuthSignInAuthenticationDefaults.Gender))
                {
                    var sex = user1.FindFirstValue(OAuthSignInAuthenticationDefaults.Gender);
                    user2.Sex = sex.Contains("男") ? SexKinds.男 : SexKinds.女;
                }

                if (user2.Avatar.IsNullOrWhiteSpace() && user1.HasClaim(h => h.Type == OAuthSignInAuthenticationDefaults.Avatar))
                {
                    var avatar = user1.FindFirstValue(OAuthSignInAuthenticationDefaults.Avatar);
                    user2.Avatar = avatar;
                }

                if (user2.DisplayName.IsNullOrWhiteSpace() && user1.HasClaim(h => h.Type == OAuthSignInAuthenticationDefaults.GivenName))
                {
                    var givenName = user1.FindFirstValue(OAuthSignInAuthenticationDefaults.GivenName);
                    user2.DisplayName = givenName;
                }
            }
        }
        #endregion

        #region 更新
        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public async Task UpdateAsync(IDictionary<string, object> dic)
        {
            var user = new TUser();

            foreach (var (key, value) in dic)
            {
                user.SetItem(key, value);
            }

            await _userManager.UpdateAsync(user);
        }

        #endregion

        #region 登录和登出

        public async Task<SignInResult> LoginAsync(string username, string password, bool rememberMe = false)
        {
            var result = await _signInManager.PasswordSignInAsync(
                username, password, rememberMe, false);

            return result;
        }

        public async Task<SignInResult> LoginAsync(IUser user, string password, bool rememberMe = false)
        {
            var result = await _signInManager.PasswordSignInAsync(user as TUser, password, rememberMe, false);

            return result;
        }

        public async Task<SignInResult> LoginAsync(IUser user)
        {
            await _signInManager.SignInAsync(user as TUser, false);

            return SignInResult.Success;
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        #endregion
    }
}
