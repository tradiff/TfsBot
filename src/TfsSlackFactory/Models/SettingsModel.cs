using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TfsSlackFactory.Models
{
    public class SettingsModel
    {
        [JsonProperty("integrations")]
        public List<SettingsIntegrationGroupModel> IntegrationGroups { get; set; } 
    }

    public class SettingsIntegrationGroupModel
    {
        public string Name { get; set; }
        public List<SettingsIntegrationModel> Integrations { get; set; }
    }

    public class SettingsIntegrationModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("white-list-query")]
        public string WhiteListQuery { get; set; }
        [JsonProperty("slack-web-hook-url")]
        public string SlackWebHookUrl { get; set; }
        [JsonProperty("format")]
        public string Format { get; set; }
        [JsonProperty("slack-channel")]
        public string SlackChannel { get; set; }
        [JsonProperty("slack-username")]
        public string SlackUsername { get; set; }
        [JsonProperty("slack-icon-emoji")]
        public string SlackIconEmoji { get; set; }
    }
}
