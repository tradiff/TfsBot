using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        public async Task<IActionResult> Post(string integration)
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
            var hookEvent = await _tfsService.CreateEventObject(json);

            foreach (var hookIntegration in _integrations.Single(x => x.Name.Equals(integration, StringComparison.CurrentCultureIgnoreCase)).Integrations)
            {
                if (hookIntegration.Type != hookEvent.EventType)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(hookIntegration.HookFilter) &&
                    !await _evalService.Eval(hookEvent, hookIntegration.HookFilter))
                {
                    continue;
                }

                var message = await _formatService.Format(hookEvent, hookIntegration.Format);
                await _slackService.PostMessage(hookIntegration.SlackWebHookUrl,
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
