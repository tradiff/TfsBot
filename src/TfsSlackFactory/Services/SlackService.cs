using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TfsSlackFactory.Models;

namespace TfsSlackFactory.Services
{
    public class SlackService
    {
        public SlackService()
        {
        }

        //Post a message using a Payload object
        public void PostMessage(string webhookUrl, SlackMessageDTO dto)
        {
            var payload = new SlackPayload
            {
                Channel = dto.Channel,
                IconEmoji = dto.IconEmoji,
                Username = dto.Username,
                Attachments = new List<SlackAttachment>
                {
                    new SlackAttachment
                    {
                        Color = dto.Color,
                        Text = dto.Text
                    }
                }
            };
            string payloadJson = JsonConvert.SerializeObject(payload);

            using (WebClient client = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                data["payload"] = payloadJson;

                var response = client.UploadValues(new Uri(webhookUrl), "POST", data);
                Encoding _encoding = new UTF8Encoding();
                //The response text is usually "ok"
                string responseText = _encoding.GetString(response);
            }
        }
    }
}