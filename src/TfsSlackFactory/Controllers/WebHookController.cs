using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TfsSlackFactory.Services;

namespace TfsSlackFactory.Controllers
{
    [Route("api/[controller]")]
    public class WebHookController : Controller
    {
        private readonly IntegrationService _integrationService;

        public WebHookController(IntegrationService integrationService)
        {
            _integrationService = integrationService;
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

                if (!_integrationService.Integrations.Any(a => String.Equals(a.Name, integration, StringComparison.CurrentCultureIgnoreCase)))
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
