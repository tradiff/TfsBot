using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json;
using TfsSlackFactory.Models;
using TfsSlackFactory.Services;

namespace TfsSlackFactory.Controllers
{
    [Route("api/[controller]")]
    public class WebHookController : Controller
    {
        private readonly TfsService _tfsService;
        private readonly List<SettingsIntegrationGroupModel> _integrations;

        public WebHookController(IOptions<List<SettingsIntegrationGroupModel>> integrations, TfsService tfsService)
        {
            _tfsService = tfsService;
            _integrations = integrations.Value;
        }

        // GET: api/values
        [HttpPost("")]
        public IActionResult Post(string integration)
        {
            if (string.IsNullOrWhiteSpace(integration))
            {
                throw new ArgumentNullException(nameof(integration));
            }

            if (!_integrations.Any(a => String.Equals(a.Name, integration, StringComparison.CurrentCultureIgnoreCase)))
            {
                return HttpBadRequest($"No integration defined that matches {integration}");
            }

            StreamReader reader = new StreamReader(Request.Body);
            var json = reader.ReadToEnd();
            var obj = JsonConvert.DeserializeObject<WorkItemHook>(json);

            _tfsService.GetWorkItem(obj.Resource.WorkItemId);

            return Ok("Test");
        }
    }
}
