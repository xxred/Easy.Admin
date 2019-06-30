
using Easy.Admin.Areas.Admin.Models;
using Microsoft.AspNetCore.Identity;
using XCode.Membership;
#if DEBUG
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Easy.Admin.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityUser = Extensions.Identity.Stores.XCode.IdentityUser;

namespace Easy.Admin.Areas.Admin.Controllers
{
    [Route("Admin/[controller]")]
    [ApiController]
    [Authorize()]
    public class AccountController : BaseController
    {
        //private readonly UserManager<User> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(//UserManager<User> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            //_userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: api/Account
        [HttpGet]
        public dynamic Get()
        {
            var identity = User.Identity as ClaimsIdentity;
            var user = new
            {
                Name = identity?.Name,
                Avatar = identity?.FindFirst(
                    //                    GithubDefaults.AvatarClaimTypes
                    "urn:qq:avatar"
                    )?.Value,
                DisplayName = identity?.Label,
                Roles = new[] { "admin" }
            };

            return user;
            //            var user = User;
            //            return new string[] { "value1", user.Identity.AuthenticationType, user.Identity.Name };
        }

        // GET: api/Account/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Account
        [HttpGet]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<JwtToken> Login([FromQuery]string username, [FromQuery]string password,
            [FromQuery]bool rememberMe = false)
        {
            var result = await _signInManager.PasswordSignInAsync(
                username, password, rememberMe, false);

            if (result.Succeeded)
            {
                var jwtToken = HttpContext.Features.Get<JwtToken>();
                return jwtToken;
            }

            throw new ApiException(2, "登陆错误");
        }

        [HttpGet]
        [Route("Authorize")]
        [AllowAnonymous]
        public dynamic AuthorizeAsync([FromQuery]string authenticationScheme, string returnUrl)
        {
            var provider = authenticationScheme;
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "http://localhost:8080/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            //if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                //throw new Exception("invalid return URL");
            }

            {
                // start challenge and roundtrip the return URL and scheme 
                var props = new AuthenticationProperties
                {
                    RedirectUri = Url.Action(nameof(Callback)),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", provider },
                    }
                };

                return Challenge(props, provider);
            }
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet]
        [Route("Challenge")]
        [AllowAnonymous]
        public Task<IActionResult> Challenge(string provider, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (Url.IsLocalUrl(returnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
            {
                //RedirectUri = Url.Action(nameof(Callback)),
                RedirectUri = returnUrl,
                Items =
                {
                    {"returnUrl", returnUrl},
                    {"scheme", provider},
                }
            };

            return Task.FromResult((IActionResult)Challenge(props, provider));
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
        [Route("Callback")]
        [AllowAnonymous]
        public IActionResult Callback()
        {
            //// read external identity from the temporary cookie
            var token = Request.Cookies.ContainsKey("Admin-Token") ? Request.Cookies["Admin-Token"] : null;
            var returnUrl = Request.Cookies.ContainsKey("returnUrl") ? Request.Cookies["returnUrl"] : null;

            if (returnUrl != null)
            {
                returnUrl += "?token=" + token;

            }
            else
            {
                returnUrl = "~/";
            }

            return Redirect(returnUrl);
        }

        [HttpPost()]
        [Route("Logout")]
        public void Logout()
        {

        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
#endif

