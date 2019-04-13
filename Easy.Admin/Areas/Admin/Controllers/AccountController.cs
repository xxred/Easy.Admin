#if DEBUG
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Easy.Admin.Areas.Admin.Controllers
{
    [Route("Admin/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AccountController : ControllerBase
    {
        public IAuthenticationSchemeProvider Schemes
        {
            get;
            set;
        }


        public AccountController(IAuthenticationSchemeProvider schemes)
        {
            if (schemes == null)
            {
                throw new ArgumentNullException("schemes");
            }

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
        public dynamic Login([FromQuery]string username, [FromQuery]string password)
        {
            var handler = new JwtSecurityTokenHandler();
            var newTokenExpiration = DateTime.Now.Add(TimeSpan.FromHours(2));
            var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name,username),
                    new Claim(ClaimTypes.NameIdentifier,"1"),
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
                //Issuer = "EasyAdminUser",
                //Audience = "EasyAdminAudience",
            });

            var encodedToken = handler.WriteToken(securityToken);

            // 为了下面重定向的时候带上token，往cookie写一份
            Response.Cookies.Append("Admin-Token", "Bearer " + encodedToken);

            return new { Token = "Bearer " + encodedToken };
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

