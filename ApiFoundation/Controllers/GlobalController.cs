using System.Collections.Generic;
using ApiFoundation.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiFoundation.Controllers
{
    [Route("/v1")]
    public class GlobalController : Controller
    {
        // GET /v1
        [HttpGet]
        public LinkedCollectionBase<GetCustomersResponse> Get()
        {
            // TODO This is hard coded but should be looking at the access control rules for the caller's bearer token
            return new LinkedCollectionBase<GetCustomersResponse>
                { Items = new List<GetCustomersResponse> {
                    new GetCustomersResponse { Customer = "root" },
                    new GetCustomersResponse { Customer = "acme" },
                } };
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

    public class GetCustomersResponse : LinkedResponse
    {
        public string Customer { get; set; }
    }
}
