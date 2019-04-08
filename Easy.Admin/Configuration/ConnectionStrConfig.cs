using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NewLife.Log;
using XCode.DataAccessLayer;

namespace Easy.Admin.Configuration
{
    /// <summary>
    /// 连接字符串配置
    /// </summary>
    public static class ConnectionStrConfig
    {
        /// <summary>
        /// 添加连接字符串
        /// </summary>
        /// <param name="services"></param>
        public static void AddConnectionStr(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionStrings = configuration.GetSection("connectionStrings");
            var connections = connectionStrings.GetChildren();
            foreach (var conn in connections)
            {
                var connStr = conn.GetSection("connectionString").Value;
                var provider = conn.GetSection("providerName").Value;

                XTrace.WriteLine($"设置连接字符串：{conn.Key} ===> {connStr} , provider ===> {provider}");

                DAL.AddConnStr(conn.Key, connStr, null, provider);
            }
        }
    }
}