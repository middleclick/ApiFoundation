using System;
using System.Threading.Tasks;
using ApiFoundation.Shared;
using ApiFoundation.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace ApiFoundation.Controllers
{
    [Route("[controller]")]
    [ApiVersion("2017-08-01")]
    public class HelloController : Controller
    {
        private readonly IDistributedCache _cache;

        public HelloController(IDistributedCache cache)
        {
            _cache = cache;
        }

        // GET /v1/{customer}/hello
        [HttpGet]
        public HelloResult Get()
        {
            return new HelloResult { Response = "Hello nobody" };
        }

        // GET /v1/{customer}/hello/5
        // This API was retired on 2017-08-31 and the return value changed.
        [HttpGet("{id:maxversion(2017-08-31)}")]
        [ApiVersion("2017-08-22")]
        [ApiExplorerSettings(IgnoreApi = true)] // necessary to prevent swagger exception
        public string Get(int id)
        {
            return "hello " + id;
        }

        // GET /v1/{customer}/values/5
        // This API was introduced on 2017-09-01; notice it has a different return type than the previous version.
        [HttpGet("{id}")]
        [ApiVersion("2017-09-01")]
        public async Task<HelloResult> Get2(string id)
        {
            var cached = await _cache.GetStringAsync(id);
            if (cached != null)
                return new HelloResult { Response = $"Hello {id}, I first saw you at {cached}" };
            
            await _cache.SetStringAsync(id, DateTime.Now.ToShortTimeString());
            return new HelloResult { Response = $"Hello {id}, nice to meet you" };            
        }
    }

    public class HelloResult : LinkedResponse
    {
        public string Response { get; set; }
    }
}
