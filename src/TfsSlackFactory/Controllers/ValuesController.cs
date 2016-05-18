using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;
using TfsSlackFactory.Models;
using TfsSlackFactory.Services;

namespace TfsSlackFactory.Controllers
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
        public string SlackTest()
        {
            var slackWebHookUrl = "https://hooks.slack.com/services/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";
            SlackService slackService = new SlackService();
            slackService.PostMessage(slackWebHookUrl, new SlackMessageDTO
            {
                Channel = "#test",
                IconEmoji = ":fire:",
                Text = "my test message",
                Username = "tfsSlackFactory",
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
