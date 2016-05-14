using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TfsSlackFactory.Models;
using TfsSlackFactory.Services;

namespace TfsSlackFactory.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
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
            var slackWebHookUrl = "https://hooks.slack.com/services/...";
            SlackService slackService = new SlackService();
            slackService.PostMessage(slackWebHookUrl, new SlackPayload
            {
                Channel = "#test",
                IconEmoji = ":fire:",
                Text = "my test message",
                Username = "tfsSlackFactory"
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
