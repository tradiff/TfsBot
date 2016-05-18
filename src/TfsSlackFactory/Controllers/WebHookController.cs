﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            foreach (var hookIntegration in _integrations.Single(x => x.Name.Equals(integration, StringComparison.CurrentCultureIgnoreCase)).Integrations)
            {
                if (hookIntegration.Type != obj.EventType)
                {
                    continue;
                }

                var workItem = _tfsService.GetWorkItem(obj.Resource.WorkItemId);

                if (!string.IsNullOrWhiteSpace(hookIntegration.WhiteListQuery) && !_tfsService.IsWorkItemInQuery(workItem.WiId, workItem.ProjectName, hookIntegration.WhiteListQuery))
                {
                   continue;
                }

                workItem.IsAssigmentChanged = obj.Resource.Fields.AssignedTo?.NewValue != obj.Resource.Fields.AssignedTo?.OldValue;
                workItem.IsStateChanged = obj.Resource.Fields.State?.NewValue != obj.Resource.Fields.State?.OldValue;

                var message = _formatService.Format(workItem, hookIntegration.Format);

                _slackService.PostMessage(hookIntegration.SlackWebHookUrl,
                    new SlackPayload
                    {
                        Channel = "#test",
                        IconEmoji = ":ghost:",
                        Username = "tfsbot",
                        Text = message
                    });
            }

            return Ok("Test");
        }
    }
}
