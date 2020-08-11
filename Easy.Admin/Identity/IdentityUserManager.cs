using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Easy.Admin.Entities;
using Easy.Admin.Localization.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewLife;
using XCode.Membership;

namespace Easy.Admin.Identity
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public class ApplicationUserManager<TUser> : UserManager<TUser> where TUser : User<TUser>, new()
    {
        /// <summary>
        /// 用于请求的语言定位器
        /// </summary>
        private IStringLocalizer<Request> _requestLocalizer;

        public ApplicationUserManager(IUserStore<TUser> store, IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TUser> passwordHasher, IEnumerable<IUserValidator<TUser>> userValidators,
            IEnumerable<IPasswordValidator<TUser>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<TUser>> logger,
            IStringLocalizer<Request> stringLocalizer) :
            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer,
                errors, services, logger)
        {
            _requestLocalizer = stringLocalizer;
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="store"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        protected override async Task<PasswordVerificationResult> VerifyPasswordAsync(IUserPasswordStore<TUser> store,
            TUser user, string password)
        {
            string text = await store.GetPasswordHashAsync(user, CancellationToken);
            if (text == null)
            {
                return PasswordVerificationResult.Failed;
            }

            if (text.Equals(password.MD5()))
            {
                return PasswordVerificationResult.Success;
            }

            Logger.LogWarning("密码不正确");
            throw new ApiException(402, _requestLocalizer["Password is incorrect"]);

            // return PasswordVerificationResult.Failed;
        }
    }
}
