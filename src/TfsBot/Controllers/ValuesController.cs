using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TfsBot.Models;
using TfsBot.Services;

namespace TfsBot.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly List<SettingsIntegrationGroupModel> _integrations;

        public ValuesController(IOptions<List<SettingsIntegrationGroupModel>> integrations)
        {
            _integrations = integrations.Value;
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var integrationGroups = _integrations;
            Console.WriteLine($"The following {integrationGroups.Count} integration groups were found:");
            foreach (var integrationGroup in integrationGroups)
            {
                Console.WriteLine(integrationGroup.Name);
            }
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // GET api/values/SlackTest
        [HttpGet("SlackTest")]
        public async Task<string> SlackTest()
        {
            var slackWebHookUrl = "https://hooks.slack.com/services/...";
            SlackService slackService = new SlackService();
            await slackService.PostMessage(slackWebHookUrl, new SlackMessageDTO
            {
                Channel = "#test",
                IconEmoji = ":fire:",
                Text = "my test message",
                Username = "tfsBot",
                Color = "#f433ff"
            });
            return "done";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
