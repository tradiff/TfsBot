using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using TfsSlackFactory.Models;
using TfsSlackFactory.Services;

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
                .AddJsonFile("appsettings.json");

            if (env.IsEnvironment("Development"))
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build().ReloadOnChanged("appsettings.json");
        }

        public static IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<List<SettingsIntegrationGroupModel>>(Configuration.GetSection("integrations"));
            services.Configure<TfsSettings>(Configuration.GetSection("tfs"));
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddTransient<FormatService, FormatService>();
            services.AddTransient<SlackService, SlackService>();
            services.AddTransient<TfsService, TfsService>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<TfsSettings> tfsSettings)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (!SettingsCheck(tfsSettings))
            {
                return;
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
