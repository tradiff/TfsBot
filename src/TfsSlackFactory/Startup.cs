using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TfsSlackFactory.Models;
using TfsSlackFactory.Services;
using Serilog;

namespace TfsSlackFactory
{
    public class Startup
    {
        public static bool Started { get; set; }
        private readonly ILogger<Startup> _logger;

        public Startup(IHostingEnvironment env, ILogger<Startup> logger)
        {
            _logger = logger;
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public static IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<List<SettingsIntegrationGroupModel>>(options => Configuration.GetSection("integrations").Bind(options));
            services.Configure<TfsSettings>(options => Configuration.GetSection("tfs").Bind(options));

            // Add framework services.

            services.AddTransient<FormatService, FormatService>();
            services.AddTransient<SlackService, SlackService>();
            services.AddTransient<TfsService, TfsService>();
            services.AddTransient<EvalService, EvalService>();
            services.AddTransient<IntegrationService, IntegrationService>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<TfsSettings> tfsSettings)
        {
            loggerFactory.AddSerilog();

            if (!SettingsCheck(tfsSettings))
            {
                var ex = new Exception("Please check your appsettings.json file");
                // I can't figure out how to log an exception using _logger, so I'm calling Serilog.Log directly here
                Serilog.Log.Error(ex, "Oops");
                throw ex;
            }

            var listeningPort = Configuration["ListeningPort"];
            var serverAddresses = app.ServerFeatures.Get<IServerAddressesFeature>();
            serverAddresses.Addresses.Clear();
            serverAddresses.Addresses.Add($"http://*:{listeningPort}/");
            _logger.LogInformation($"Magic happens at http://*:{listeningPort}/");

            app.UseMvc();
            Started = true;
        }

        private bool SettingsCheck(IOptions<TfsSettings> tfsSettings)
        {
            bool result = true;
            if (string.IsNullOrWhiteSpace(tfsSettings.Value.Server))
            {
                _logger.LogError("appsettings.json is missing the value for tfs\\server");
                result = false;
            }
            if (string.IsNullOrWhiteSpace(tfsSettings.Value.Username))
            {
                _logger.LogError("appsettings.json is missing the value for tfs\\username");
                result = false;
            }
            if (string.IsNullOrWhiteSpace(tfsSettings.Value.Password))
            {
                _logger.LogError("appsettings.json is missing the value for tfs\\password");
                result = false;
            }

            return result;
        }
    }
}
