using System.Collections.Generic;

namespace TfsSlackFactory.Models
{

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
        // todo: move SelfUrl 1 level up
        public string SelfUrl { get; set; }
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
