using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EdgeHelpers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Exceptions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace EdgeDriverLoader.Demo
{
    class Program
    {
        private static IServiceProvider _serviceProvider;
        private ILogger _logger;
        static void Main(string[] args)
        {
            try
            {
                RegisterServices();
                Log.Information("Building...");
                var webdriverFinder = _serviceProvider.GetService<EdgeWebDriverFinder>();
                var webdriverpath = webdriverFinder.Configure().FindAvailableDrivers().FindEdgeVersion().ReturnMatchedDriverPath();
                Log.ForContext<Program>().Information($"Found driver {webdriverpath}");
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Log.ForContext<Program>().Error(ex, ex.Message);
                Thread.Sleep(5000);
            }
        }


        private static void RegisterServices()
        {
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddSerilog(BuildSerilog()));
            services.AddScoped<EdgeWebDriverFinder>();
            _serviceProvider = services.BuildServiceProvider();
        }
        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }
        private static Serilog.ILogger BuildSerilog()
        {
            var logger = new LoggerConfiguration()
                .Enrich.WithExceptionDetails()
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs/log.txt"), rollingInterval: RollingInterval.Day)
                .WriteTo.File(Path.Combine("S:\\Sovellukset\\Tax_technology\\Development\\DA tiimi\\Logs", "EdgeDriverLoaderTest/log.txt"), rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .MinimumLevel.Information()
                .CreateLogger();

            Log.Logger = logger;

            return logger;
        }
    }
}
