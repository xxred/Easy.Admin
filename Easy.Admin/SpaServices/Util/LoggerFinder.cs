using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Easy.Admin.SpaServices.Util
{
    internal static class LoggerFinder
    {
        public static ILogger GetOrCreateLogger(
            IApplicationBuilder appBuilder,
            string logCategoryName)
        {
            // If the DI system gives us a logger, use it. Otherwise, set up a default one
            var logger = appBuilder.ApplicationServices.GetService(typeof(ILoggerFactory)) is ILoggerFactory loggerFactory
                ? loggerFactory.CreateLogger(logCategoryName)
                : NullLogger.Instance;
            return logger;
        }
    }
}
