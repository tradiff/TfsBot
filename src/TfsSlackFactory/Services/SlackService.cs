using System;
using System.Collections.Generic;
using System.Net.Http;
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
        public async Task PostMessage(string webhookUrl, SlackMessageDTO dto)
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

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("payload", payloadJson)
                    }
                );
                var result = client.PostAsync(webhookUrl, content).Result;
                string resultContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(resultContent);
            }
        }
    }
}