using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Easy.Admin.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Easy.Admin.Common
{
    /// <summary>
    /// 验证码助手
    /// </summary>
    public static class VerCodeHelper
    {
        /// <summary>
        /// 手机缓存key
        /// </summary>
        private const string RedisKeyPhoneCode = "admin:phone:";

        /// <summary>
        /// 邮箱缓存key
        /// </summary>
        private const string RedisKeyMailCode = "admin:mail:";

        private static IServiceProvider _serviceProvider;

        private static IRedisService _redisService;
        /// <summary>
        /// 缓存服务
        /// </summary>
        private static IRedisService RedisService =>
            _redisService ??= (IRedisService)_serviceProvider.GetService(typeof(IRedisService));

        private static IPhoneMessage _phoneMessage;
        /// <summary>
        /// 发送手机信息服务
        /// </summary>
        private static IPhoneMessage PhoneMessage =>
            _phoneMessage ??= (IPhoneMessage)_serviceProvider.GetService(typeof(IPhoneMessage));

        private static IMailMessage _mailMessage;
        /// <summary>
        /// 发送邮箱信息服务
        /// </summary>
        private static IMailMessage MailMessage =>
            _mailMessage ??= (IMailMessage)_serviceProvider.GetService(typeof(IMailMessage));

        /// <summary>
        /// 添加验证模块
        /// </summary>
        /// <param name="app"></param>
        public static void AddVerCode(this IApplicationBuilder app)
        {
            _serviceProvider = app.ApplicationServices;
        }

        /// <summary>
        /// 发送手机验证码，使用前请实现<see cref="IPhoneMessage"/>并注入
        /// </summary>
        /// <param name="key">手机号</param>
        public static void PhoneSendVerCode(string key)
        {
            var code = GenerateVerCode();

            //验证码绑定手机号并存储到redis
            RedisService.Set(RedisKeyPhoneCode + key, code);
            RedisService.Expire(RedisKeyPhoneCode + key, 300);

            // 发送验证码
            PhoneMessage.Send(key, code);
        }

        /// <summary>
        /// 发送邮箱验证码，使用前请实现<see cref="IMailMessage"/>并注入
        /// </summary>
        /// <param name="key">邮箱</param>
        public static void MailSendVerCode(string key)
        {
            var code = GenerateVerCode();

            //验证码绑定邮箱并存储到redis
            RedisService.Set(RedisKeyMailCode + key, code);
            RedisService.Expire(RedisKeyMailCode + key, 300);

            // 发送验证码
            MailMessage.Send(key, code);
        }

        /// <summary>
        /// 检查验证码是否正确
        /// </summary>
        /// <param name="key"></param>
        /// <param name="code"></param>
        /// <param name="type">类型，0-手机，1-邮箱</param>
        /// <returns></returns>
        public static bool CheckVerCode(string key, string code, int type)
        {
            var realCode = type switch
            {
                0 => RedisService.Get(RedisKeyPhoneCode + key),
                1 => RedisService.Get(RedisKeyMailCode + key),
                _ => ""
            };

            return code == realCode;
        }

        private static string GenerateVerCode()
        {
            var sb = new StringBuilder();
            var random = new Random();
            for (var i = 0; i < 6; i++)
            {
                sb.Append(random.Next(10));
            }

            var code = sb.ToString();

            return code;
        }
    }
}
