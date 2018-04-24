using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TfsBot.Models;
using TfsBot.Services;

namespace TfsBot.Controllers
{
    [Route("api/[controller]")]
    public class WebHookController : Controller
    {
        private readonly IntegrationService _integrationService;
        private readonly SettingsModel _settings;

        public WebHookController(IntegrationService integrationService, IOptions<SettingsModel> settings)
        {
            _integrationService = integrationService;
            _settings = settings.Value;
        }

        [HttpPost("")]
        public async Task<IActionResult> Post(string integration)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(integration))
                {
                    throw new ArgumentNullException(nameof(integration));
                }

                if (!_settings.IntegrationGroups.Any(a => String.Equals(a.Name, integration, StringComparison.CurrentCultureIgnoreCase)))
                {
                    throw new ArgumentException($"No integration defined that matches {integration}");
                }

                StreamReader reader = new StreamReader(Request.Body);
                var json = reader.ReadToEnd();
                await _integrationService.ProcessEvent(integration, json);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error occured");
            }

            return Ok();
        }
    }
}
