using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TfsBot.Models;
using TfsBot.Services;

namespace TfsBot
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.Configure<SettingsModel>(options => Configuration.Bind(options));

            // Add framework services.
            services.AddTransient<FormatService, FormatService>();
            services.AddTransient<SlackService, SlackService>();
            services.AddTransient<TfsService, TfsService>();
            services.AddTransient<EvalService, EvalService>();
            services.AddTransient<IntegrationService, IntegrationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, TfsService tfsService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            tfsService.SetupSubscriptions();

            app.UseMvc();
        }
    }
}
