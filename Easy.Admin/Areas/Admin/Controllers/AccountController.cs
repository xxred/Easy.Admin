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
    //[Authorize]
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
        public IEnumerable<string> Get()
        {
            var user = User;
            return new string[] { "value1", user.Identity.AuthenticationType, user.Identity.Name };
        }

        // GET: api/Account/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Account
        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public dynamic Login([FromForm]string username, [FromForm]string password)
        {
            var handler = new JwtSecurityTokenHandler();
            var newTokenExpiration = DateTime.Now.Add(TimeSpan.FromHours(2));
            var identity = new ClaimsIdentity(
                new GenericIdentity(username, "TokenAuth"),
                new[] { new Claim("ID", "1") }
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

            var client = new MongoClient(new MongoUrl("mongodb://hebinghong.com:27017")
            {

            });
            var db = client.GetDatabase("EasyAdmin");
            var doc = db.GetCollection<AccessToken>("AccessToken");
            doc.InsertOne(new AccessToken()
            {
                Token = encodedToken
            });

            return new { Token = "Bearer " + encodedToken };
        }

        [HttpGet("{authenticationScheme}")]
        [Route("Authorize")]
        [AllowAnonymous]
        public async Task<dynamic> AuthorizeAsync(string authenticationScheme)
        {
            var context = HttpContext;
            context.Features.Set((IAuthenticationFeature)new AuthenticationFeature
            {
                OriginalPath = context.Request.Path,
                OriginalPathBase = context.Request.PathBase
            });

            var handlers = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();

            foreach (AuthenticationScheme item in await Schemes.GetRequestHandlerSchemesAsync())
            {
                var authenticationRequestHandler = (await handlers.GetHandlerAsync(context, item.Name))
                    as IAuthenticationRequestHandler;
                bool flag = authenticationRequestHandler != null;
                if (flag)
                {
                    flag = await authenticationRequestHandler.HandleRequestAsync();
                }
                if (flag)
                {
                    return null;
                }
            }
            //AuthenticationScheme authenticationScheme = await Schemes.GetDefaultAuthenticateSchemeAsync();
            if (authenticationScheme != null)
            {
                var authenticateResult = await context.AuthenticateAsync(authenticationScheme);
                if (authenticateResult?.Principal != null)
                {
                    context.User = authenticateResult.Principal;
                }
            }

            return new { Token = "Bearer "  };
        }

        // PUT: api/Account/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
