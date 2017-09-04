using System;
using System.Threading.Tasks;
using ApiFoundation.Shared;
using ApiFoundation.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace ApiFoundation.Controllers
{
    /// <summary>
    /// Controller for /v1/Hello/... routes
    /// </summary>
    [Route("[controller]")]
    [ApiVersion("2017-08-01")]
    public class HelloController : CommonController
    {
        private readonly IDistributedCache _cache;

        /// <summary>
        /// ctor
        /// </summary>
        public HelloController(IDistributedCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Welcomes the caller to our API.
        /// </summary>
        [HttpGet]
        public HelloAllResult Get()
        {
            return new HelloAllResult {
                Response = $"Hello {HttpContext.User.Identity.Name}",
                Items = new [] {
                    new PersonICanSayHelloTo { Id = "bob" },
                    new PersonICanSayHelloTo { Id = "foo" }
                }
            };
        }

        /// <summary>
        /// Welcomes the caller to our API, by the name they passed in.
        /// </summary>
        /// <param name="id">Name of the person calling</param>
        /// <returns>A welcome message</returns>
        /// <remarks>This API was retired on 2017-08-31 and the return value changed.</remarks>
        [HttpGet("{id:maxversion(2017-08-31)}")]
        [ApiVersion("2017-08-22")]
        [ApiExplorerSettings(IgnoreApi = true)] // necessary to prevent swagger exception
        public string Get(int id)
        {
            return "Hello";
        }

        /// <summary>
        /// Welcomes the caller to our API, by the name they passed in.
        /// </summary>
        /// <param name="id">Name of the person calling</param>
        /// <param name="customer"></param>
        /// <returns>A welcome message</returns>
        /// <remarks>
        /// If you are adapting code written prior to 2017-09-01, notice that the API
        /// has a different return type than the previous version.  Whereas before the
        /// method returned a simple string, now it returns an object with a property
        /// "Response" which contains the greeting message.
        /// </remarks>
        [HttpGet("{id}", Name = "Hello_Get_id")]
        [ApiVersion("2017-09-01")]
        public async Task<HelloResult> Get2(string id, string customer)
        {
            var cached = await _cache.GetStringAsync(id);
            if (cached != null)
                return new HelloResult { Response = $"Hello {id} from {customer}, I first saw you at {cached}" };
            
            await _cache.SetStringAsync(id, DateTime.Now.ToShortTimeString());
            return new HelloResult { Response = $"Hello {id} from {customer}, nice to meet you" };            
        }

        /// <summary>
        /// Can the caller get it?  Only if it is foo.
        /// </summary>
        [NonAction]
        public bool CanGet2(string id)
        {
            return id == "foo";
        }
    }

    /// <summary>
    /// A successful response from GET Hello
    /// </summary>
    public class HelloResult : LinkedResponse
    {
        /// <summary>
        /// A welcoming message to the caller.
        /// </summary>
        public string Response { get; set; }
    }

    public class HelloAllResult : LinkedCollectionBase<PersonICanSayHelloTo>
    {
        public string Response { get; set; }
    }

    public class PersonICanSayHelloTo : LinkedResponse
    {
        public string Id { get; set; }
    }
}
