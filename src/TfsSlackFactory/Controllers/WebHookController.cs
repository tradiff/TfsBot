using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        private readonly EvalService _evalService;
        private readonly List<SettingsIntegrationGroupModel> _integrations;

        public WebHookController(IOptions<List<SettingsIntegrationGroupModel>> integrations, FormatService formatService, SlackService slackService, TfsService tfsService, EvalService evalService)
        {
            _formatService = formatService;
            _slackService = slackService;
            _tfsService = tfsService;
            _evalService = evalService;
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
                return BadRequest($"No integration defined that matches {integration}");
            }

            StreamReader reader = new StreamReader(Request.Body);
            var json = reader.ReadToEnd();
            var obj = JsonConvert.DeserializeObject<WorkItemHook>(json);

            foreach (var hookIntegration in _integrations.Single(x => x.Name.Equals(integration, StringComparison.CurrentCultureIgnoreCase)).Integrations)
            {
                if (hookIntegration.Type != obj.EventType)
                {
                    continue;
                }

                var workItem = _tfsService.GetWorkItem(obj);

                if (!string.IsNullOrWhiteSpace(hookIntegration.HookFilter) &&
                    !_evalService.Eval(workItem, hookIntegration.HookFilter))
                {
                    continue;
                }

                var message = _formatService.Format(workItem, hookIntegration.Format);
                _slackService.PostMessage(hookIntegration.SlackWebHookUrl,
                    new SlackMessageDTO
                    {
                        Channel = hookIntegration.SlackChannel,
                        IconEmoji = hookIntegration.SlackIconEmoji,
                        Username = hookIntegration.SlackUsername,
                        Text = message,
                        Color = hookIntegration.SlackColor
                    });
            }

            return Ok("Test");
        }
    }
}
