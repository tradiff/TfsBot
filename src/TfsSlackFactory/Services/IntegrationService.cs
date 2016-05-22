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

        public async Task ProcessEvent(string input)
        {
            var hookEvent = await _tfsService.CreateEventObject(input);

            if (hookEvent == null)
            {
                return;
            }

            foreach (var hookIntegration in Integrations.Single(x => x.Name.Equals(hookEvent.EventType, StringComparison.CurrentCultureIgnoreCase)).Integrations)
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
        }
    }
}