using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace ApiFoundation.Versioning
{
    internal class ApiVersionRouteConstraint : IRouteConstraint
    {
        public ApiVersionRouteConstraint(string maxVersion)
        {
            if (string.IsNullOrEmpty(maxVersion))
                throw new ArgumentNullException(nameof(maxVersion));
            DateTime date;
            if (!DateTime.TryParseExact(maxVersion, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                throw new ArgumentException("Invalid version format", nameof(maxVersion));
            MaxDateVersion = date;
        }

        public DateTime MaxDateVersion { get; }

        public bool Match(
            HttpContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            return DoesRequestVersionMatch(httpContext, MaxDateVersion);
        }

        public static bool DoesRequestVersionMatch(HttpContext httpContext, DateTime maxDateVersion)
        {
            StringValues apiVersion;

            // If the caller didn't request any version specifically, then don't match
            // this route.  This is because the route has a maximum API version which
            // is by definition, in the past, and the caller is implicitly asking for
            // the current version of the API.
            if (!httpContext.Request.Headers.TryGetValue("x-api-version", out apiVersion)
                || apiVersion.Count < 1)
                return false;
            
            // Find the version of the API that the caller asked for
            DateTime date;
            if (!DateTime.TryParseExact(apiVersion[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                throw new Exception("Bad version header");
            
            return date < maxDateVersion;
        }
    }
}