using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog.Events;
using TfsBot.Models;

namespace TfsBot.Services
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
                var response = await client.PostAsync(webhookUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();
                var logLevel = response.IsSuccessStatusCode ? LogEventLevel.Information : LogEventLevel.Warning;
                Serilog.Log.Write(logLevel, $"Slack returned code: {(int)response.StatusCode} {response.StatusCode}");
                Serilog.Log.Write(logLevel, responseString);
            }
        }
    }
}