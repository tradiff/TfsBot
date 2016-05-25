using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.RollingFile;

namespace TfsSlackFactory
{
    public class Program 
    {
        public static void Main(string[] args)
        {
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.RollingFile(Path.Combine("logs", "log-{Date}.txt"))
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Serilog.Log.Information("Starting TfsSlackFactory");


            var hostingApplication = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            hostingApplication.Run();
        }
    }
}
