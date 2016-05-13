using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;

namespace TfsSlackFactory.Controllers
{
    [Route("api/[controller]")]
    public class WebHookController : Controller
    {
        // GET: api/values
        [HttpPost("")]
        public IActionResult Post()
        {
            StreamReader reader = new StreamReader(Request.Body);
            var json = reader.ReadToEnd();
            var obj = JsonConvert.DeserializeObject(json);
            return Ok("Test");
        }
    }
}
