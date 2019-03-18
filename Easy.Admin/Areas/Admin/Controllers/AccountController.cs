using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Authentication.Github;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace Easy.Admin.Areas.Admin.Controllers
{
    [Route("Admin/[controller]")]
    [ApiController]
    [Authorize]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AccountController : ControllerBase
    {
        public IAuthenticationSchemeProvider Schemes
        {
            get;
            set;
        }

        private readonly IIdentityServerInteractionService _interaction;


        public AccountController(IIdentityServerInteractionService interaction,
            IAuthenticationSchemeProvider schemes)
        {
            if (schemes == null)
            {
                throw new ArgumentNullException("schemes");
            }

            _interaction = interaction;
            Schemes = schemes;
        }

        // GET: api/Account
        [HttpGet]
        public dynamic Get()
        {
            var identity = User.Identity as ClaimsIdentity;
            var user = new
            {
                Name = identity?.Name,
                Avatar = identity?.FindFirst(GithubDefaults.AvatarClaimTypes)?.Value,
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
        public dynamic Login([FromQuery]string username, [FromQuery]string password, [FromQuery]string returnUrl)
        {
            var handler = new JwtSecurityTokenHandler();
            var newTokenExpiration = DateTime.Now.Add(TimeSpan.FromHours(2));
            var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name,username),
                    new Claim(ClaimTypes.NameIdentifier,"1"),
                    new Claim(IdentityModel.JwtClaimTypes.Subject, "1"),
                    new Claim("idp","auto"),
                }, "TokenAuth"
            );

            var secretKey = "EasyAdminEasyAdminEasyAdmin";
            var signingKey = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                SecurityAlgorithms.HmacSha256);

            var securityToken = handler.CreateToken(new SecurityTokenDescriptor()
            {
                SigningCredentials = signingKey,
                Subject = identity,
                Expires = newTokenExpiration,
                Issuer = "EasyAdminUser",
                Audience = "EasyAdminAudience",
            });

            var encodedToken = handler.WriteToken(securityToken);

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                // 为了下面重定向的时候带上token，往cookie写一份
                Response.Cookies.Append("token", "Bearer " + encodedToken);
                Response.Redirect(returnUrl);
            }

            return new { Token = "Bearer " + encodedToken };
        }

        [HttpGet]
        [Route("Authorize")]
        [AllowAnonymous]
        public async Task<dynamic> AuthorizeAsync([FromQuery]string authenticationScheme, string returnUrl)
        {
            var provider = authenticationScheme;
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "http://localhost:8080/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            //if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                //throw new Exception("invalid return URL");
            }

            //if (AccountOptions.WindowsAuthenticationSchemeName == provider)
            //{
            //    // windows authentication needs special handling
            //    return await ProcessWindowsLoginAsync(returnUrl);
            //}
            //else
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
            if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
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
        public async Task<IActionResult> Callback()
        {
            //// read external identity from the temporary cookie
            var token = Request.Cookies.ContainsKey("token") ? Request.Cookies["token"] : null;
            var returnUrl = Request.Cookies.ContainsKey("returnUrl") ? Request.Cookies["returnUrl"] : null;

            if (returnUrl != null)
            {
                returnUrl += "?token=" + token;
            }
            else
            {
                returnUrl = "~/";
            }

            //if (result?.Succeeded != true)
            //{
            //    throw new Exception("External authentication error");
            //}

            //// lookup our user and external provider info
            //var (user, provider, providerUserId, claims) = FindUserFromExternalProvider(result);
            //if (user == null)
            //{
            //    // this might be where you might initiate a custom workflow for user registration
            //    // in this sample we don't show how that would be done, as our sample implementation
            //    // simply auto-provisions new external user
            //    user = AutoProvisionUser(provider, providerUserId, claims);
            //}

            //// this allows us to collect any additonal claims or properties
            //// for the specific prtotocols used and store them in the local auth cookie.
            //// this is typically used to store data needed for signout from those protocols.
            //var additionalLocalClaims = new List<Claim>();
            //var localSignInProps = new AuthenticationProperties();
            //ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);
            //ProcessLoginCallbackForWsFed(result, additionalLocalClaims, localSignInProps);
            //ProcessLoginCallbackForSaml2p(result, additionalLocalClaims, localSignInProps);

            //// issue authentication cookie for user
            //await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.SubjectId, user.Username));
            //await HttpContext.SignInAsync(user.SubjectId, user.Username, provider, localSignInProps, additionalLocalClaims.ToArray());

            //// delete temporary cookie used during external authentication
            //await HttpContext.SignOutAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // retrieve return URL
            //var returnUrl = "/";//result.Properties.Items["returnUrl"] ?? "~/";

            //// check if external login is in the context of an OIDC request
            //var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            //if (context != null)
            //{
            //    if (await _clientStore.IsPkceClientAsync(context.ClientId))
            //    {
            //        // if the client is PKCE then we assume it's native, so this change in how to
            //        // return the response is for better UX for the end user.
            //        return View("Redirect", new RedirectViewModel { RedirectUrl = returnUrl });
            //    }
            //}

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
