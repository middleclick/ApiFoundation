using System.Collections.Generic;
using ApiFoundation.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiFoundation.Controllers
{
    [Route("/v1")]
    public class GlobalController : CommonController
    {
        // GET /v1
        [HttpGet]
        public LinkedResponse Get()
        {
            // The only thing needed here is the links.  There isn't any other data to return.
            return new LinkedResponse();
        }
    }
}
