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
        Task<IUser> FindByIdAsync(string userId);

        Task<IManageUser> GetOrCreateUserAsync(ClaimsPrincipal user, AuthenticationProperties properties,
            bool createUserOnOAuthLogin);

        Task<SignInResult> LoginAsync(string username, string password, bool rememberMe = false);

        Task SignOutAsync();
    }
}
