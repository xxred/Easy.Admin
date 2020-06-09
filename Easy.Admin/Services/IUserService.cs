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
        Task<IdentityResult> CreateAsync(string[] names, object[] values);

        Task<IdentityResult> CreateAsync(string userName, string password);

        Task<IUser> FindByEmailAsync(string email);

        Task<IUser> FindByIdAsync(string userId);

        Task<IUser> FindByLoginAsync(string loginProvider, string providerKey);

        Task<IUser> FindByNameAsync(string name);

        Task<IUser> FindByPhoneNumberAsync(string phoneNumber);

        Task<IManageUser> GetOrCreateUserAsync(ClaimsPrincipal user, AuthenticationProperties properties,
            bool createUserOnOAuthLogin);

        Task<SignInResult> LoginAsync(string username, string password, bool rememberMe = false);

        Task<SignInResult> LoginAsync(IUser user, string password, bool rememberMe = false);

        Task<SignInResult> LoginAsync(IUser user);

        Task SignOutAsync();
    }
}
