using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TfsSlackFactory
{
    public class Program : ServiceBase
    {
        public IConfigurationRoot Configuration { get; set; }
        private IApplication _hostingApplication;

        public void Main(string[] args)
        {
            if (args.Contains("--windows-service"))
            {
                Run(this);
            }
            else
            {
                Console.WriteLine("Starting console mode");
                OnStart(null);
                if (Startup.Started)
                {
                    Console.WriteLine("Started the server");
                    Console.WriteLine("To exit, press any key");
                    Console.ReadKey(true);
                }
                Console.WriteLine("Stopping server...");
                OnStop();
            }
        }

        protected override void OnStart(string[] args)
        {
            // Set up configuration sources.
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");

            configBuilder.AddEnvironmentVariables();

            // note: this changes in RC2:
            // https://github.com/aspnet/Announcements/issues/168
            _hostingApplication = new WebHostBuilder(configBuilder.Build())
                .UseServer("Microsoft.AspNet.Server.Kestrel")
                .Build()
                .Start();

        }

        protected override void OnStop()
        {
            if (_hostingApplication != null)
                _hostingApplication.Dispose();
            Console.WriteLine("Server stopped");
        }
    }
}
