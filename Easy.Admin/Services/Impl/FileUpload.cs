using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Easy.Admin.Services.Impl
{
    public class DefaultFileUpload : IFileUpload
    {
        private readonly IWebHostEnvironment _env;
        private FileUploadOptions _options;

        public DefaultFileUpload(IOptions<FileUploadOptions> config, IWebHostEnvironment env)
        {
            IOptions<FileUploadOptions> options = config;
            if (options == null)
                throw new ArgumentNullException(nameof(config));
            this._options = options.Value;
            this._env = env;
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="key">文件路径，不以斜杠开头</param>
        /// <param name="content">文件内容</param>
        /// <returns></returns>
        public string PutObject(string key, Stream content)
        {
            var imgUrl = Path.Combine(_options.SaveFileDir, key);
            var saveFilePath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath + "/wwwroot", imgUrl);
            var fileInfo = new FileInfo(saveFilePath);

            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                //判断目录是否存在
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            if (fileInfo.Exists)
            {
                // 已存在则删除
                fileInfo.Delete();
            }

            var fs = new FileStream(saveFilePath, FileMode.Create);
            content.CopyTo(fs);
            fs.Flush();
            fs.Dispose();

            return _options.Url + imgUrl.Replace('\\', '/');
        }
    }

    public class FileUploadOptions
    {
        /// <summary>
        /// 文件保存路径
        /// </summary>
        public string SaveFileDir { get; set; }

        /// <summary>
        /// 拼接的文件域名，以斜杠结尾
        /// </summary>
        public string Url { get; set; }
    }

    public static class FileUploadExtensions
    {
        /// <summary>
        /// 添加默认文件上传模块
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddDefaultFileUpload(
            this IServiceCollection services,
            Action<FileUploadOptions> configAction)
        {
            services.Configure(configAction);
            services.TryAddSingleton<IFileUpload, DefaultFileUpload>();
            return services;
        }
    }
}
