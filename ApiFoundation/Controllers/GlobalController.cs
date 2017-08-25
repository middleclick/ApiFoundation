using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiFoundation.Models;
using ApiFoundation.ResourceLinking;
using Microsoft.AspNetCore.Mvc;

namespace ApiFoundation.Controllers
{
    [Route("/v1")]
    public class GlobalController : Controller
    {
        // GET /v1
        [HttpGet]
        public CollectionBase<string> Get()
        {
            // TODO This is hard coded but should be looking at the access control rules for the caller's bearer token
            return new CollectionBase<string> { Items = new List<string> { "root", "acme" } };
        }

        // GET /v1/{customer}
        [HttpGet("{customer}")]
        public LinkedResponse GetTopLevel(string customer)
        {
            // TODO should make sure the caller has access to the called customer.
            
            // The only thing needed here is the links.  There isn't any other data to return.
            return new LinkedResponse();
        }
    }
}
