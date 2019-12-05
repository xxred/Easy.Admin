using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Easy.Admin.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XCode.Membership;

namespace Easy.Admin.Identity
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public class ApplicationUserManager<TUser> : UserManager<TUser> where TUser : User<TUser>, new()
    {
        public ApplicationUserManager(IUserStore<TUser> store, IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TUser> passwordHasher, IEnumerable<IUserValidator<TUser>> userValidators,
            IEnumerable<IPasswordValidator<TUser>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<TUser>> logger) :
            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer,
                errors, services, logger)
        {
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
            throw new ApiException(402, "密码不正确");

            // return PasswordVerificationResult.Failed;
        }
    }
}
