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
        private readonly FormatService _formatService;
        private readonly SlackService _slackService;
        private readonly TfsService _tfsService;
        private readonly List<SettingsIntegrationGroupModel> _integrations;

        public WebHookController(IOptions<List<SettingsIntegrationGroupModel>> integrations, FormatService formatService, SlackService slackService, TfsService tfsService)
        {
            _formatService = formatService;
            _slackService = slackService;
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

            // todo: loop through integrations

            // todo: check wiql

            var dynamicWorkItem = _tfsService.GetWorkItem(obj.Resource.WorkItemId);

            //todo: get parent work item as dynamic also

            //todo: build SlackWorkItemModel from the two dynamics

            // mock SlackWorkItemModel:
            var swim = new SlackWorkItemModel
            {
                WiId = "1234",
                WiTitle = "Test Task",
                WiUrl = "https://tfs.datalinksoftware.com/tfs/DefaultCollection/CareBook/_workitems#_a=edit&id=23357",
                DisplayName = "Fred Flintstone",
                ParentWiId = "123",
                ParentWiTitle = "Test PBI",
                ParentWiType = "Bug",
                ParentWiUrl = "https://tfs.datalinksoftware.com/tfs/DefaultCollection/CareBook/_workitems#_a=edit&id=23357"
            };

            var message =
                "<{parentWiUrl}|{parentWiType} {parentWiId}: {parentWiTitle}> > <{wiUrl}|Task {wiId}: {wiTitle}> completed by {displayName}";

            message = _formatService.Format(swim, message);

            _slackService.PostMessage("https://hooks.slack.com/services/...",
                new SlackPayload
                {
                    Channel = "#test",
                    IconEmoji = ":ghost:",
                    Username = "tfsbot",
                    Text = message
                });

            return Ok("Test");
        }
    }
}
