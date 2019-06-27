using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MagmaConverse
{
    public class Program
    {
        public static void Main(string[] args)
        {
            InitLogging();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

#if USE_WEBHOST
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
#endif

        internal static void InitLogging()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());

            var currAppDir = AppDomain.CurrentDomain.BaseDirectory;
            var logfile = new FileInfo(currAppDir + "log4net.config");
            XmlConfigurator.Configure(logRepository, logfile);
        }
    }
}
