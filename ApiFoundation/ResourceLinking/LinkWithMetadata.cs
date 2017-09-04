using ApiFoundation.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApiFoundation.ResourceLinking
{
    class LinkWithMetadata
    {
        public Link Link { get; set; }

        // The callback should both check delegated admin permissions on the route
        // itself, and also call the "CanXXX" method related to the route handler,
        // if one is present.
        public CheckAvailabilityDelegate CheckAvailability { get; set; }

        // (Context, link, resource identifier parameters) => available / not available
        public delegate bool CheckAvailabilityDelegate(HttpContext context, Controller controller, RouteData routeData);
    }
}
