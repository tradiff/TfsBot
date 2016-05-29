using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TfsSlackFactory.Models;

namespace TfsSlackFactory.Services
{
    public class IntegrationService
    {
        public List<SettingsIntegrationGroupModel> Integrations { get; }
        private readonly TfsService _tfsService;
        private readonly FormatService _formatService;
        private readonly EvalService _evalService;
        private readonly SlackService _slackService;

        public IntegrationService(IOptions<List<SettingsIntegrationGroupModel>> integrations, TfsService tfsService, FormatService formatService, EvalService evalService, SlackService slackService)
        {
            Integrations = integrations.Value;
            _tfsService = tfsService;
            _formatService = formatService;
            _evalService = evalService;
            _slackService = slackService;
        }

        public async Task ProcessEvent(string integration, string input)
        {
            var hookEvent = await _tfsService.CreateEventObject(input);

            if (hookEvent == null)
            {
                return;
            }

            var integrationGroup = Integrations.Single(x => x.Name.Equals(integration, StringComparison.CurrentCultureIgnoreCase));
            if (integrationGroup.EventType != hookEvent.EventType)
            {
                return;
            }

            foreach (var hookIntegration in integrationGroup.Integrations)
            {
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
        }
    }
}