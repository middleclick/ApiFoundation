using ApiFoundation.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiFoundation.Controllers
{
    /// <summary>
    /// Global routes that are handled by the framework rather than by plugins.
    /// </summary>
    [Route("/v1")]
    public class GlobalController : CommonController
    {
        /// <summary>
        /// Get the list of links available to the caller at the top level of the API.
        /// </summary>
        [HttpGet]
        public LinkedResponse Get()
        {
            // The only thing needed here is the links.  There isn't any other data to return.
            return new LinkedResponse();
        }
    }
}
