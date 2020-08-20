using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Areas.Admin.RequestParams;
using Easy.Admin.Areas.Admin.ResponseParams;
using Easy.Admin.Authentication.ExternalSignIn;
using Easy.Admin.Authentication.JwtBearer;
using Easy.Admin.Authentication.OAuthSignIn;
using Easy.Admin.Common;
using Easy.Admin.Configuration;
using Easy.Admin.Entities;
using Easy.Admin.Localization.Resources;
using Easy.Admin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NewLife;
using NewLife.Log;
using NewLife.Reflection;
using NewLife.Serialization;
using XCode.Membership;

namespace Easy.Admin.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("账号")]
    public class AccountController : AdminControllerBase
    {
        private readonly JwtBearerAuthenticationOptions _authenticationOptions;
        private readonly OAuthConfiguration _oAuthConfiguration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IUserService _userService;



        public AccountController(IOptions<JwtBearerAuthenticationOptions> authenticationOptions,
            OAuthConfiguration oAuthConfiguration,
            IHttpClientFactory clientFactory, IUserService userService
            )
        {
            _authenticationOptions = authenticationOptions.Value;
            _oAuthConfiguration = oAuthConfiguration;
            _clientFactory = clientFactory;
            _userService = userService;
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public ApiResult<ResponseUserInfo> GetUserInfo()
        {
            //var principal = User;
            var identity = User.Identity as IUser;

            if (identity == null)
            {
                throw ApiException.Common(RequestLocalizer["User type error"], 500);
            }

            var data = new ResponseUserInfo();
            data.Copy(identity);

            return ApiResult.Ok(data);
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <returns></returns>
        [HttpPut("[action]")]
        public ApiResult<bool> UpdateUserInfo(RequestUserInfo userInfo)
        {
            if (userInfo.ID < 1)
            {
                throw ApiException.Common(RequestLocalizer["Incorrect ID"]);
            }

            var u = AppUser;

            if (IsSupperAdmin || u.ID == userInfo.ID)
            {
                _userService.UpdateAsync(userInfo.ToDictionary());
            }
            else
            {
                throw ApiException.Common(RequestLocalizer["No permission"]);
            }

            return ApiResult.Ok(true);
        }

        /// <summary>
        /// 修改密码
        /// Type:0-旧密码，1-手机验证码，2-邮箱验证码
        /// 0-传新旧密码
        /// 1-传新密码和验证码，区号，手机号
        /// 2-传新密码和验证码，邮箱
        /// </summary> 
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("[action]")]
        public ApiResult<bool> ChangePassword(RequestPassword model)
        {
            switch (model.Type)
            {
                case 0:
                    if (AppUser.Password != model.OldPassword.MD5())
                    {
                        throw ApiException.Common(RequestLocalizer["The old password is incorrect"]);
                    }
                    break;
                case 1:
                    if (!CheckVerCode(model.InternationalAreaCode + model.Mobile, model.VerCode, 0))
                    {
                        throw ApiException.Common(RequestLocalizer["The verification code is incorrect or expired"]);
                    }

                    if (AppUser.Mobile != model.Mobile)
                    {
                        throw ApiException.Common(RequestLocalizer["Incorrect mobile phone number"]);
                    }
                    break;
                case 2:
                    if (!CheckVerCode(model.Mail, model.VerCode, 1))
                    {
                        throw ApiException.Common(RequestLocalizer["The verification code is incorrect or expired"]);
                    }

                    if (AppUser.Mail != model.Mail)
                    {
                        throw ApiException.Common(RequestLocalizer["Incorrect email address"]);
                    }
                    break;
                default:
                    throw ApiException.Common("Type类型不正确！");
            }

            AppUser.Password = model.NewPassword;
            AppUser.Save();

            return ApiResult.Ok(true);
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="rememberMe"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<ApiResult<JwtToken>> Login([FromQuery] string username, [FromQuery] string password, [FromQuery] string culture = "en-US",
            [FromQuery] bool rememberMe = false)
        {
            var result = await _userService.LoginAsync(username, password, rememberMe);

            if (result.Succeeded)
            {
                var jwtToken = HttpContext.Features.Get<JwtToken>();
                return ApiResult.Ok(jwtToken);
            }

            throw new ApiException(402, RequestLocalizer["Wrong account or password"]);
        }

        /// <summary>
        /// 登录，包含四种模式，设置参数Type选择
        /// 0-用户名密码，1-手机密码，2-手机验证码，3-邮箱密码，4-邮箱验证码
        /// </summary>
        /// <param name="model"></param>
        /// <param name="culture">语言</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<ApiResult<JwtToken>> Login(RequestRegister model, [FromQuery] string culture)
        {
            IUser user;
            Microsoft.AspNetCore.Identity.SignInResult result;

            switch (model.Type)
            {
                case 1:
                    user = await _userService.FindByPhoneNumberAsync(model.Mobile);
                    result = await _userService.LoginAsync(user, model.Password);
                    break;
                case 2:
                    if (!CheckVerCode(model.InternationalAreaCode + model.Mobile, model.VerCode, 0))
                    {
                        throw ApiException.Common(RequestLocalizer["The verification code is incorrect or expired"]);
                    }
                    user = await _userService.FindByPhoneNumberAsync(model.Mobile);
                    result = await _userService.LoginAsync(user);
                    break;
                case 3:
                    user = await _userService.FindByEmailAsync(model.Mail);
                    result = await _userService.LoginAsync(user, model.Password);
                    break;
                case 4:
                    if (!CheckVerCode(model.Mail, model.VerCode, 1))
                    {
                        throw ApiException.Common(RequestLocalizer["The verification code is incorrect or expired"]);
                    }
                    user = await _userService.FindByEmailAsync(model.Mail);
                    result = await _userService.LoginAsync(user);
                    break;
                case 0:
                default:
                    user = await _userService.FindByNameAsync(model.UserName);
                    result = await _userService.LoginAsync(user, model.Password);
                    break;
            }

            if (result.Succeeded)
            {
                var jwtToken = HttpContext.Features.Get<JwtToken>();
                return ApiResult.Ok(jwtToken);
            }

            throw ApiException.Common(RequestLocalizer["Wrong account or password"]);
        }


        /// <summary>
        /// 注册，包含四种模式，设置参数Type选择
        /// 0-用户名密码，1-手机密码，2-手机验证码，3-邮箱密码，4-邮箱验证码
        /// </summary>
        /// <param name="model"></param>
        /// <param name="culture">语言</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<ApiResult<string>> Register(RequestRegister model, [FromQuery] string culture)
        {
            string[] names;
            object[] values;

            switch (model.Type)
            {
                case 1:
                case 2:
                    if (!CheckVerCode(model.InternationalAreaCode + model.Mobile, model.VerCode, 0))
                    {
                        throw ApiException.Common(RequestLocalizer["The verification code is incorrect or expired"]);
                    }
                    names = new[]
                    {
                        nameof(IUser.Name),
                        nameof(IUser.DisplayName),
                        nameof(IUser.Mobile),
                        nameof(IUser.Password),
                    };
                    // 模式2，不带密码，默认密码由内部生成，应用应该引导用户修改密码后使用
                    values = new object[] { model.Mobile, model.Mobile, model.Mobile, model.Password };
                    break;
                case 3:
                case 4:
                    if (!CheckVerCode(model.Mail, model.VerCode, 1))
                    {
                        throw ApiException.Common(RequestLocalizer["The verification code is incorrect or expired"]);
                    }
                    names = new[]
                    {
                        nameof(IUser.Name),
                        nameof(IUser.DisplayName),
                        nameof(IUser.Mail),
                        nameof(IUser.Password),
                    };
                    values = new object[] { model.Mail, model.Mail, model.Mail, model.Password };
                    break;
                case 0:
                default:
                    names = new[]
                    {
                        nameof(IUser.Name),
                        nameof(IUser.DisplayName),
                        nameof(IUser.Password),
                    };
                    values = new object[] { model.UserName, model.UserName, model.Password };
                    break;
            }

            var result = await _userService.CreateAsync(names, values);

            if (result.Succeeded)
            {
                return ApiResult.Ok();
            }

            var err = result.Errors.ToArray()[0];
            XTrace.WriteLine($"创建用户发生错误：{err.Description}");

            throw ApiException.Common(RequestLocalizer[err.Code]);
        }

        /// <summary>
        /// 根据授权码向外部身份验证机构认证，颁发本系统token，用于swagger
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetToken")]
        [AllowAnonymous]
        public async Task<ActionResult> GetTokenAsync([FromForm] string grant_type, [FromForm] string code, [FromForm] string client_id, [FromForm] string redirect_uri)
        {
            #region 获取token
            var tokenEndpoint = _oAuthConfiguration.Authority.EnsureEnd("/") + "connect/token";
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
            requestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { nameof(grant_type),grant_type },
                { nameof(code),code },
                { nameof(client_id),client_id },
                { nameof(redirect_uri),redirect_uri },
            });

            requestMessage.Headers.Add("Authorization", Request.Headers["Authorization"].ToString());

            var backchannel = _clientFactory.CreateClient();
            var responseMessage = await backchannel.SendAsync(requestMessage);

            var contentMediaType = responseMessage.Content.Headers.ContentType?.MediaType;
            if (string.IsNullOrEmpty(contentMediaType))
            {
                XTrace.WriteLine($"Unexpected token response format. Status Code: {(int)responseMessage.StatusCode}. Content-Type header is missing.");
            }
            else if (!string.Equals(contentMediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                XTrace.WriteLine($"Unexpected token response format. Status Code: {(int)responseMessage.StatusCode}. Content-Type {responseMessage.Content.Headers.ContentType}.");
            }

            // Error handling:
            // 1. If the response body can't be parsed as json, throws.
            // 2. If the response's status code is not in 2XX range, throw OpenIdConnectProtocolException. If the body is correct parsed,
            //    pass the error information from body to the exception.
            OpenIdConnectMessage message;
            try
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                message = new OpenIdConnectMessage(responseContent);
            }
            catch (Exception ex)
            {
                throw new OpenIdConnectProtocolException($"Failed to parse token response body as JSON. Status Code: {(int)responseMessage.StatusCode}. Content-Type: {responseMessage.Content.Headers.ContentType}", ex);
            }

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new ApiException(500, message.Error);
            }
            #endregion

            #region 解析token，颁发本系统token

            var token = new JwtSecurityTokenHandler().ReadJwtToken(message.AccessToken);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(token.Claims));
            var props = new AuthenticationProperties
            {
                Items =
                {
                    {"scheme", _oAuthConfiguration.Scheme},
                }
            };

            await HttpContext.SignInAsync(OAuthSignInAuthenticationDefaults.AuthenticationScheme, principal, props);

            var jwtToken = HttpContext.Features.Get<JwtToken>();

            var tokenResult = new
            {
                access_token = jwtToken.Token.Replace(message.TokenType + " ", ""),
                expires_in = message.ExpiresIn,
                token_type = message.TokenType
            };

            return Content(tokenResult.ToJson(), "application/json");
            #endregion
        }

        /// <summary>
        /// 重定向到外部身份验证提供程序的链接
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
                throw ApiException.Common(RequestLocalizer["Invalid return URL"]);
            }

            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
                //RedirectUri = returnUrl,
                Items =
                {
                    {"returnUrl", returnUrl},
                    {"scheme", provider},
                }
            };

            return Task.FromResult((IActionResult)Challenge(props, provider));
        }

        /// <summary>
        /// 第三方登录
        /// </summary>
        /// <param name="loginProvider">提供者</param>
        /// <param name="providerKey">token</param>
        /// <returns></returns>
        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<ApiResult<JwtToken>> ExternalLogin(string loginProvider, string providerKey)
        {
            var props = new AuthenticationProperties
            {
                Items =
                {
                    {"scheme", loginProvider},
                    {"providerKey", providerKey},
                }
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity());

            await HttpContext.SignInAsync(ExternalSignInDefaults.AuthenticationScheme, user, props);

            var jwtToken = HttpContext.Features.Get<JwtToken>();

            return ApiResult.Ok(jwtToken);
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
        [Route("Callback")]
        [AllowAnonymous]
        public IActionResult Callback()
        {
            var tokenKey = _authenticationOptions.TokenKey;
            var returnUrlKey = _authenticationOptions.ReturnUrlKey;

            //// read external identity from the temporary cookie
            var token = Request.Cookies.ContainsKey(tokenKey) ? Request.Cookies[tokenKey] : null;
            var returnUrl = Request.Cookies.ContainsKey(returnUrlKey) ? Request.Cookies[returnUrlKey] : null;

            if (returnUrl != null)
            {
                returnUrl += "?token=" + token;

            }
            else
            {
                returnUrl = "~/";
            }

            Response.Cookies.Delete(returnUrlKey);

            return Redirect(returnUrl);
        }

        /// <summary>
        /// 解绑第三方账号
        /// </summary>
        /// <param name="id">绑定列表id</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public ApiResult<string> UnbindOAuth([FromQuery] string id)
        {
            var uc = UserConnect.FindByKey(id);
            if (uc == null)
            {
                throw ApiException.Common(RequestLocalizer["Data not found"]);
            }

            if (!IsSupperAdmin && uc.UserID != AppUser.ID)
            {
                throw ApiException.Common(RequestLocalizer["No permission"]);
            }

            uc.Delete();

            return ApiResult.Ok();
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [HttpPost()]
        [Route("Logout")]
        public async Task<ApiResult<string>> Logout()
        {
            await _userService.SignOutAsync();

            return ApiResult.Ok();
        }

        /// <summary>
        /// 根据类型获取验证码。类型，0-手机，1-邮箱
        /// </summary>
        /// <param name="key">手机或邮箱，手机要带区号，比如8615777777777</param>
        /// <param name="type">类型，0-手机，1-邮箱</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        [AllowAnonymous]
        public ApiResult<bool> GetVerCode(string key, int type)
        {
            switch (type)
            {
                case 1:
                    VerCodeHelper.MailSendVerCode(key);
                    break;
                case 0:
                default:
                    VerCodeHelper.PhoneSendVerCode(key);
                    break;
            }

            return ApiResult.Ok(true);
        }

        /// <summary>
        /// 检查验证码。类型，0-手机，1-邮箱
        /// </summary>
        /// <param name="key"></param>
        /// <param name="code"></param>
        /// <param name="type">类型，0-手机，1-邮箱</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        [AllowAnonymous]
        public bool CheckVerCode(string key, string code, int type)
        {
            return VerCodeHelper.CheckVerCode(key, code, type);
        }
    }
}
