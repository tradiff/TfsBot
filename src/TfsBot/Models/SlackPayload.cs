using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TfsBot.Models
{
    public class SlackMessageDTO
    {
        public string Channel { get; set; }
        public string Username { get; set; }
        public string IconEmoji { get; set; }
        public string Text { get; set; }
        public string Color { get; set; }
    }

    //This class serializes into the Json payload required by Slack Incoming WebHooks
    public class SlackPayload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("icon_emoji")]
        public string IconEmoji { get; set; }

        [JsonProperty("attachments")]
        public List<SlackAttachment> Attachments { get; set; } 
    }

    public class SlackAttachment
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }
}
