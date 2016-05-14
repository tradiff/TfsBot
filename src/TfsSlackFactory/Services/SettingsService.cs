using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TfsSlackFactory.Models;

namespace TfsSlackFactory.Services
{
    public sealed class SettingsService
    {
        #region Singleton
        private static readonly Lazy<SettingsService> lazy = new Lazy<SettingsService>(() => new SettingsService());

        public static SettingsService Instance { get { return lazy.Value; } }
        #endregion


        public SettingsModel Settings { get; internal set; }

        private SettingsService()
        {
            GetSettings();
        }

        private void GetSettings()
        {
            var integrationGroups = Startup.Configuration.GetSection("integrations").GetChildren().ToList();

            this.Settings = new SettingsModel();
            this.Settings.IntegrationGroups = new List<SettingsIntegrationGroupModel>();

            foreach (var integrationGroup in integrationGroups)
            {
                var settingIg = new SettingsIntegrationGroupModel();
                this.Settings.IntegrationGroups.Add(settingIg);
                settingIg.Integrations = new List<SettingsIntegrationModel>();
                settingIg.Name = integrationGroup.Key;

                var integrations = integrationGroup.GetChildren();
                foreach (var integration in integrations)
                {
                    var settingIntegration = new SettingsIntegrationModel();
                    settingIg.Integrations.Add(settingIntegration);
                    settingIntegration.Type = integration["type"];
                    settingIntegration.WhiteListQuery = integration["white-list-query"];
                    settingIntegration.SlackWebHookUrl = integration["slack-web-hook-url"];
                    settingIntegration.Format = integration["format"];
                    settingIntegration.SlackChannel = integration["slack-channel"];
                    settingIntegration.SlackUsername = integration["slack-username"];
                    settingIntegration.SlackIconEmoji = integration["slack-icon-emoji"];
                }
            }
        }
    }
}
