using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Easy.Admin.SpaServices.Npm;
using Easy.Admin.SpaServices.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.Logging;

namespace Easy.Admin.SpaServices
{
    // 参考 https://github.com/aspnet/AspNetCore/blob/master/src/Middleware/SpaServices.Extensions/src/ReactDevelopmentServer/ReactDevelopmentServerMiddleware.cs
    internal static class VueDevelopmentServerMiddleware
    {
        private const string LogCategoryName = "Microsoft.AspNetCore.SpaServices";
        private static TimeSpan RegexMatchTimeout = TimeSpan.FromSeconds(5); // This is a development-time only feature, so a very long timeout is fine

        public static void Attach(
            ISpaBuilder spaBuilder,
            string npmExe,
            string npmScriptName)
        {
            var sourcePath = spaBuilder.Options.SourcePath;
            if (string.IsNullOrEmpty(sourcePath))
            {
                throw new ArgumentException("Cannot be null or empty", nameof(sourcePath));
            }

            if (string.IsNullOrEmpty(npmScriptName))
            {
                throw new ArgumentException("Cannot be null or empty", nameof(npmScriptName));
            }

            if (string.IsNullOrEmpty(npmExe))
            {
                throw new ArgumentException("Cannot be null or empty", nameof(npmExe));
            }

            // Start create-vue-app and attach to middleware pipeline
            var appBuilder = spaBuilder.ApplicationBuilder;
            var logger = LoggerFinder.GetOrCreateLogger(appBuilder, LogCategoryName);
            var portTask = StartCreateVueAppServerAsync(sourcePath, npmExe, npmScriptName, logger);

            // Everything we proxy is hardcoded to target http://localhost because:
            // - the requests are always from the local machine (we're not accepting remote
            //   requests that go directly to the create-Vue-app server)
            // - given that, there's no reason to use https, and we couldn't even if we
            //   wanted to, because in general the create-Vue-app server has no certificate
            var targetUriTask = portTask.ContinueWith(
                task => new UriBuilder("http", "localhost", task.Result).Uri);

            spaBuilder.UseProxyToSpaDevelopmentServer(() =>
            {
                // On each request, we create a separate startup task with its own timeout. That way, even if
                // the first request times out, subsequent requests could still work.
                var timeout = spaBuilder.Options.StartupTimeout;
                return targetUriTask.WithTimeout(timeout,
                    $"The create-Vue-app server did not start listening for requests " +
                    $"within the timeout period of {timeout.Seconds} seconds. " +
                    $"Check the log output for error information.");
            });
        }

        private static async Task<int> StartCreateVueAppServerAsync(
            string sourcePath, string npmExe, string npmScriptName, ILogger logger)
        {
            var portNumber = TcpPortFinder.FindAvailablePort();
            logger.LogInformation($"Starting create-vue-app server on port {portNumber}...");

            var envVars = new Dictionary<string, string>
            {
                { "PORT", portNumber.ToString() },
                { "BROWSER", "none" }, // We don't want create-vue-app to open its own extra browser window pointing to the internal dev server port
            };
            //var arguments = $"--port={portNumber}";
            var npmScriptRunner = new NpmScriptRunner(
                sourcePath,npmExe, npmScriptName, null, envVars);
            npmScriptRunner.AttachToLogger(logger);

            using (var stdErrReader = new EventedStreamStringReader(npmScriptRunner.StdErr))
            {
                try
                {
                    // 尽管vue dev服务器最终可能会告诉我们它监听的URL，
                    // 在编译完成之前，它不会这样做，即使在没有编译器警告的情况下也不会这样做。
                    // 所以与其等着，还不如在它开始倾听请求时尽快准备好。
                    await npmScriptRunner.StdOut.WaitForMatch(
                        new Regex("App running at", RegexOptions.None, RegexMatchTimeout));
                }
                catch (EndOfStreamException ex)
                {
                    throw new InvalidOperationException(
                        $"The NPM script '{npmScriptName}' exited without indicating that the " +
                        $"create-vue-app server was listening for requests. The error output was: " +
                        $"{stdErrReader.ReadAsString()}", ex);
                }
            }

            return portNumber;
        }
    }
}
