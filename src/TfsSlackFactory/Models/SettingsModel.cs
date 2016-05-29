using System.Collections.Generic;

namespace TfsSlackFactory.Models
{
    public class SettingsModel
    {
        public int ListeningPort { get; set; }
        public string SelfUrl { get; set; }
        public string SelfName { get; set; }
        public TfsSettings Tfs { get; set; }
        public List<SettingsIntegrationGroupModel> IntegrationGroups { get; set; }
    }

    public class TfsSettings
    {
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SettingsIntegrationGroupModel
    {
        public string Name { get; set; }
        // todo: use TFS project name instead of guid
        public string TfsProject { get; set; }
        public string EventType { get; set; }
        public List<SettingsIntegrationModel> Integrations { get; set; }
    }

    public class SettingsIntegrationModel
    {
        public string HookFilter { get; set; }
        public string SlackWebHookUrl { get; set; }
        public string Format { get; set; }
        public string SlackChannel { get; set; }
        public string SlackUsername { get; set; }
        public string SlackIconEmoji { get; set; }
        public string SlackColor { get; set; }
    }
}
