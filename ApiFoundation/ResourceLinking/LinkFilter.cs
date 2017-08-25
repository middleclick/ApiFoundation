using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ApiFoundation.Shared.Models;
using ApiFoundation.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;

namespace ApiFoundation.ResourceLinking
{
    internal class LinkFilter : IResultFilter
    {
        public LinkFilter(IActionDescriptorCollectionProvider provider)
        {
            // Note that there is a possible race condition here; two threads
            // could set _routeDict to two different lazy initializers.
            // Odds are very high that both threads would see the last
            // initializer that was set, by the time they attempt to access
            // the lazy variable (and thus force its evaluation).
            // In the worst case scenario:
            // * Thread 1 sees _routeDict null, then gets put to sleep.
            // * Thread 2 sees _routeDict null, then somehow runs to completion
            //   (including, the API being fully executed, the response returned,
            //   and the links inside of the response being evaluated), without
            //   Thread 1 getting a chance to execute again.
            // * Thread 1 gets a chance to execute again, and throws away the
            //   already-evaluated route dictionary, causing it to be evaluated
            //   a second time.
            //
            // Much more likely, even in a race condition situation:
            // * Thread 1 sees _routeDict null, then gets put to sleep.
            // * Thread 2 sees _routeDict null, creates the lazy evaluator
            //   (but doesn't evaluate it).  Then gets put to sleep.
            // * Thread 1 continues and changes the lazy evaluator.
            // * Threads 1 and 2 both see the same lazy evaluator, and it
            //   gets evaluated exactly once.
            // So, this situation is unlikely to happen, and has no negative
            // consequence other than a slightly slower execution until the
            // _routeDict evaluator reaches quiescence.
            // 
            // This could be fixed with a synchronization context, but that
            // would have a performance impact every time an API is executed
            // instead of just once on startup.
            if (_routeDict == null)
            {
                _routeDict = new Lazy<IDictionary<string, IList<Link>>>(
                    () => {
                        var routeDict = new Dictionary<string, IList<Link>>();
                        var routes = provider.ActionDescriptors.Items
                            .Where(r => !string.IsNullOrEmpty(r.AttributeRouteInfo?.Template))
                            .Select(x => new Link {
                                Name = GetName(x),
                                Href = x.AttributeRouteInfo.Template,
                                Method = GetMethod(x),
                            })
                            .ToList();
                        
                        // Map from a route, to all related routes
                        foreach (var route in routes)
                        {
                            IList<Link> segRoutes;
                            if (!routeDict.TryGetValue(route.Href, out segRoutes))
                            {
                                segRoutes = new List<Link>();
                                routeDict.Add(route.Href, segRoutes);
                            }
                            segRoutes.Add(route);

                            var lastSep = route.Href.LastIndexOf('/');
                            if (lastSep > 0)
                            {
                                var parent = route.Href.Substring(0, lastSep);
                                if (!routeDict.TryGetValue(parent, out segRoutes))
                                {
                                    segRoutes = new List<Link>();
                                    routeDict.Add(parent, segRoutes);
                                }
                                segRoutes.Add(route);
                            }
                        }
                        return routeDict;
                    }
                );
            }
        }

        private static string GetName(ActionDescriptor ad)
        {
            if (!string.IsNullOrEmpty(ad.AttributeRouteInfo.Name))
                return ad.AttributeRouteInfo.Name;
            
            var name = $"{ad.RouteValues["Controller"]}_{ad.RouteValues["Action"]}";
            foreach (var param in ad.Parameters)
            {
                if (!string.Equals(param.Name, "customer", StringComparison.InvariantCultureIgnoreCase))
                    name += $"_{param.Name}";
            }
            return name;
        }

        private static string GetMethod(ActionDescriptor ad)
        {
            var result = ad.ActionConstraints?.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods.FirstOrDefault();
            return result == "GET" ? null : result;
        }

        private static Lazy<IDictionary<string, IList<Link>>> _routeDict;

        private static Regex _maxVersionRex = new Regex(@":maxversion\((\d\d\d\d-\d\d-\d\d)\)", RegexOptions.Compiled);

        public void OnResultExecuted(ResultExecutedContext context)
        {
            // Can't do much here.
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            var objectResult = context.Result as ObjectResult;
            if (objectResult == null)
                return;
            
            var obj = objectResult.Value as LinkedResponse;
            if (obj == null)
                return;
            
            obj.Links = obj.Links ?? new List<Link>();

            // Get this route's info
            var ad = context.ActionDescriptor;
            if (ad == null)
                return;
            var thisRoute = ad.AttributeRouteInfo?.Template;
            if (thisRoute == null)
                return;
            var thisName = GetName(ad);
            var thisMethod = GetMethod(ad);

            // Get all related routes
            IList<Link> routes;
            if (!_routeDict.Value.TryGetValue(thisRoute, out routes))
                return;

            // Fill in related route information
            foreach (var route in routes)
            {
                // We don't bother with "self" links.  They seem to be useless.
                if (route.Name == thisName)
                    continue;

                // If the service gave us information for a route, leave it untouched.
                if (obj.Links.Any(l => l.Name == route.Name))
                    continue;
                
                // If a link has a maxversion, see if the caller has access to it; skip otherwise
                var newLink = new Link {Name = route.Name, Href = route.Href, Method = route.Method};
                var match = _maxVersionRex.Match(route.Href);
                if (match.Success)
                {
                    var maxVersion = match.Groups[1].Value;
                    DateTime date;
                    if (!DateTime.TryParseExact(maxVersion, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                        throw new ArgumentException("Invalid version format", nameof(maxVersion));

                    if (!ApiVersionRouteConstraint.DoesRequestVersionMatch(context.HttpContext, date))
                        continue;

                    // cut the maxversion out
                    newLink.Href = route.Href.Substring(0, match.Captures[0].Index) + route.Href.Substring(match.Captures[0].Index + match.Captures[0].Length);
                }

                obj.Links.Add(newLink);
            }

            // Normalize the links to provide accurate links to the caller
            obj.Links = obj.Links
                // Remove routes that don't have an Href, because those are inaccessible.
                .Where(l => l.Href != null)
                // Remove duplicates; for those, the first item is the only one accessible.
                .GroupBy(l => l.Href).Select(g => g.First())
                // Convert back into a list
                .ToList();
        }
    }
}
