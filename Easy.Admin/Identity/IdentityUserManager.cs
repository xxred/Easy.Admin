using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Easy.Admin.Identity
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger) :
            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer,
                errors, services, logger)
        {
        }

        public override async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            user.Password = password.MD5();

            return await CreateAsync(user);
            //return await Store.CreateAsync(user, CancellationToken);
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="store"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        protected override async Task<PasswordVerificationResult> VerifyPasswordAsync(IUserPasswordStore<ApplicationUser> store,
            ApplicationUser user, string password)
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
