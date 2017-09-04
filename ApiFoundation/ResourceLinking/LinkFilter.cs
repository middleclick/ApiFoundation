using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ApiFoundation.Rbac;
using ApiFoundation.Shared.Models;
using ApiFoundation.Shared.Rbac;
using ApiFoundation.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;

namespace ApiFoundation.ResourceLinking
{
    internal class LinkFilter : IResultFilter
    {
        public LinkFilter(IActionDescriptorCollectionProvider provider, IServiceProvider serviceProvider)
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
                _routeDict = new Lazy<RouteDictionary>(
                    () => RouteDictionary.Create(provider, serviceProvider)
                );
            }
        }

        private static Lazy<RouteDictionary> _routeDict;

        public void OnResultExecuting(ResultExecutingContext context)
        {
            var objectResult = context.Result as ObjectResult;
            if (objectResult == null)
                return;

            var obj = objectResult.Value as LinkedResponse;
            if (obj == null)
                return;

            var routeData = (context.Controller as Controller)?.RouteData;

            // Make sure all of the link collections are initialized
            obj.Links = obj.Links ?? new List<Link>();
            var coll = obj as ILinkedCollectionBase<LinkedResponse>;
            if (coll != null)
            {
                foreach (var item in coll.GetItems())
                {
                    item.Links = item.Links ?? new List<Link>();
                }
            }

            // Get the currently executing route's info
            var ad = context.ActionDescriptor;
            if (ad == null)
                return;
            var thisRouteHref = ad.AttributeRouteInfo?.Template;
            if (thisRouteHref == null)
                return;
            var thisRouteName = RouteDictionary.GetRouteName(ad);
            var thisRouteMethod = RouteDictionary.GetRouteMethod(ad);

            // Get all related routes
            IList<LinkWithMetadata> routes;
            if (!_routeDict.Value.TryGetValue(thisRouteHref, out routes))
                return;

            // Fill in related route information
            foreach (var route in routes)
            {
                var routeLink = route.Link;

                // We don't bother with "self" links.  They seem to be useless.
                if (routeLink.Name == thisRouteName)
                    continue;

                // If the service gave us information for a route, leave it untouched.
                if (obj.Links.Any(l => l.Name == routeLink.Name))
                    continue;

                // If a link has a maxversion, see if the caller has access to it; skip otherwise.
                // Note that this may modify the link href.
                if (!ApiVersionLinkFilter.CheckLink(ref routeLink, context.HttpContext))
                    continue;

                // If the object is a collection, and the link is parameterized in
                // the last segment, add the link to all of the children, and skip
                // the link at the collection level.  This isn't perfect; it is
                // possible that a link might be "v1/{customer}/widget/{customer}"
                // for example; i.e. the last segment of the link may be
                // parameterized on a value that isn't part of the object.  But it
                // is probably the 99% case.
                if (coll != null)
                {
                    var splitAt = routeLink.Href.LastIndexOf('/') + 1;
                    var lastPart = routeLink.Href.Substring(splitAt);
                    if (_parameterized.IsMatch(lastPart))
                    {
                        foreach (var item in coll.GetItems())
                        {
                            var subLink = routeLink;
                            if (IsRouteAvailableToCaller(route, ref subLink, context.HttpContext, context.Controller as Controller, routeData, item))
                            {
                                item.Links.Add(subLink);
                            }
                        }
                        continue;
                    }
                }
                
                // Note that this may modify the route link
                if (IsRouteAvailableToCaller(route, ref routeLink, context.HttpContext, context.Controller as Controller, routeData, obj))
                    obj.Links.Add(routeLink);
            }

            FixLinks(obj);
            if (coll != null)
            {
                foreach (var item in coll.GetItems())
                {
                    FixLinks(item);
                }
            }
        }


        public void OnResultExecuted(ResultExecutedContext context)
        {
            // Nothing to do here.
        }

        private void FixLinks(LinkedResponse obj)
        {
            // Normalize the links to provide accurate links to the caller
            obj.Links = obj.Links
                // Remove routes that don't have an Href, because those are inaccessible.
                .Where(l => l.Href != null)
                // Remove duplicates; for those, the first item is the only one accessible.
                .GroupBy(l => l.Href).Select(g => g.First())
                // Convert back into a list
                .ToList();
        }

        private static bool IsRouteAvailableToCaller(LinkWithMetadata route, ref Link link, HttpContext httpContext, Controller controller, RouteData routeData, object objectContext)
        {
            var snapshot = routeData.PushState(null, null, null);
            try
            {
                var routeParams = routeData.Values;

                PropertyInfo[] properties = null;

                var matches = _parameterized.Matches(route.Link.Href);
                foreach (var match in matches.OfType<Match>())
                {
                    var parm = match.Groups[1].Value;

                    object value;
                    if (!routeParams.TryGetValue(parm, out value))
                    {
                        if (properties == null)
                            properties = objectContext.GetType().GetProperties();
                        var propInfo = properties.FirstOrDefault(p => StringComparer.OrdinalIgnoreCase.Equals(p.Name, parm));
                        if (propInfo != null)
                        {
                            value = propInfo.GetValue(objectContext);
                            routeParams[parm] = value;
                        }
                    }
                    if (value != null)
                    {
                        link = link.WithHref(href => Regex.Replace(href, _parameterizedPattern, value.ToString(), RegexOptions.IgnoreCase));
                    }
                }

                if (route.CheckAvailability != null &&
                    !route.CheckAvailability(httpContext, controller, routeData))
                    return false;

                return true;
            }
            finally
            {
                snapshot.Restore();
            }
        }

        private static readonly string _parameterizedPattern = @"\{([^\}:]+)(:[^\}]+)?\}";
        private static readonly Regex _parameterized = new Regex(_parameterizedPattern, RegexOptions.Compiled);
    }
}
