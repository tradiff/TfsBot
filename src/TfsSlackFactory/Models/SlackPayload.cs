using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TfsSlackFactory.Models
{
    //This class serializes into the Json payload required by Slack Incoming WebHooks
    public class SlackPayload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("icon_emoji")]
        public string IconEmoji { get; set; }
    }
}
