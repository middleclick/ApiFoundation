using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ApiFoundation.Controllers
{
    [Route("/v1/root/global")]
    public class GlobalController : Controller
    {
        // GET api/values
        [HttpGet("config")]
        public JsonResult Get()
        {
            return new JsonResult(new { name = "value 1" });
        }
    }
}
