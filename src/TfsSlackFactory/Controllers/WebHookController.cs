using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using TfsSlackFactory.Models;

namespace TfsSlackFactory.Controllers
{
    [Route("api/[controller]")]
    public class WebHookController : Controller
    {
        // GET: api/values
        [HttpPost("")]
        public IActionResult Post(string @event)
        {
            StreamReader reader = new StreamReader(Request.Body);
            var json = reader.ReadToEnd();
            var obj = JsonConvert.DeserializeObject<RootObject>(json);
            return Ok("Test");
        }
    }
}
