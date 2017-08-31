using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ApiFoundation.Shared.Models;
using ApiFoundation.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;

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
                _routeDict = new Lazy<IDictionary<string, IList<LinkWithMetadata>>>(
                    () => {
                        var routeDict = new Dictionary<string, IList<LinkWithMetadata>>();
                        var routes = provider.ActionDescriptors.Items
                            .Where(r => !string.IsNullOrEmpty(r.AttributeRouteInfo?.Template))
                            .Select(x => new Link {
                                Name = GetRouteName(x),
                                Href = x.AttributeRouteInfo.Template,
                                Method = GetRouteMethod(x),
                            })
                            .ToList();

                        // Map from a route, to all related routes
                        foreach (var route in routes)
                        {
                            AddRouteLink(routeDict, route.Href, route);
                            
                            var lastSep = route.Href.LastIndexOf('/');
                            if (lastSep > 0)
                            {
                                var parent = route.Href.Substring(0, lastSep);
                                AddRouteLink(routeDict, parent, route);
                            }
                        }
                        return routeDict;
                    }
                );
            }
        }

        private static void AddRouteLink(Dictionary<string, IList<LinkWithMetadata>> routeDict, string route, Link link)
        {
            IList<LinkWithMetadata> segRoutes;
            if (!routeDict.TryGetValue(route, out segRoutes))
            {
                segRoutes = new List<LinkWithMetadata>();
                routeDict.Add(route, segRoutes);
            }
            segRoutes.Add(new LinkWithMetadata { Link = link });
        }

        private static string GetRouteName(ActionDescriptor ad)
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

        private static string GetRouteMethod(ActionDescriptor ad)
        {
            var result = ad.ActionConstraints?.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods.FirstOrDefault();
            return result == "GET" ? null : result;
        }

        private static Lazy<IDictionary<string, IList<LinkWithMetadata>>> _routeDict;

        private static Regex _parameterized = new Regex(@"\{([^\}]+)\}", RegexOptions.Compiled);

        public void OnResultExecuting(ResultExecutingContext context)
        {
            var objectResult = context.Result as ObjectResult;
            if (objectResult == null)
                return;

            var obj = objectResult.Value as LinkedResponse;
            if (obj == null)
                return;

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
            var thisRouteName = GetRouteName(ad);
            var thisRouteMethod = GetRouteMethod(ad);

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

                // If the object is a collection, and the link is parameterized in
                // the last portion, add the link to all of the children, and skip
                // the link at the collection level.  This isn't perfect; it is
                // possible that a link might be "v1/{customer}/widget/{customer}"
                // for example; i.e. the last portion of the link may be
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
                            //AddLinkIfAvailableToCaller(item.Links, route.Duplicate());
                            item.Links.Add(routeLink.Duplicate());
                        }
                        continue;
                    }
                }
                
                // We manipulate the link and don't want to alter the route table we saved.
                var newLink = routeLink.Duplicate();

                // If a link has a maxversion, see if the caller has access to it; skip otherwise
                if (!ApiVersionLinkFilter.CheckLink(newLink, context.HttpContext))
                    continue;

                obj.Links.Add(newLink);
            }

            var actual = context.HttpContext.Request.Path.ToString().TrimStart('/');
            FixLinks(obj, thisRouteHref, actual, null);
            if (coll != null)
            {
                PropertyInfo[] properties = null;
                foreach (var item in coll.GetItems())
                {
                    // Assumption is that every item in the collection is of the same type as the first one.
                    properties = properties ?? item.GetType().GetProperties();
                    FixLinks(item, thisRouteHref, actual, properties);
                }
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            // Can't do much here.
        }


        private void FixLinks(LinkedResponse obj, string template, string actual, PropertyInfo[] propertyInfo)
        {
            // Normalize the links to provide accurate links to the caller
            obj.Links = obj.Links
                // Remove routes that don't have an Href, because those are inaccessible.
                .Where(l => l.Href != null)
                // Remove duplicates; for those, the first item is the only one accessible.
                .GroupBy(l => l.Href).Select(g => g.First())
                // Remove any links that the caller does not have access to.
                .Where(l => CallerHasAccess(l.Href))
                // Replace parameters
                .Select(l => ReplaceParameters(l, obj, template, actual, propertyInfo))
                // Convert back into a list
                .ToList();
        }

        private bool CallerHasAccess(string href)
        {
            // TBD.  Most likely this will require a callback per-plugin.  The callback should
            // be passed in the route's implementing method info and the parameters to be
            // filled in when the route is called.  The callback should be done once for the
            // batch of all links, allowing the callee to optimize whatever access control
            // checks they need to perform.
            return true;
        }

        private Link ReplaceParameters(Link l, LinkedResponse obj, string template, string actual, PropertyInfo[] propertyInfo)
        {
            l.Href = l.Href.Replace(template, actual);

            if (propertyInfo != null)
            {
                var match = _parameterized.Match(l.Href);
                if (match.Success)
                {
                    var parm = match.Groups[1].Value;
                    if (parm != null)
                    {
                        var prop = propertyInfo.FirstOrDefault(pi => StringComparer.OrdinalIgnoreCase.Equals(pi.Name, parm));
                        var val = prop?.GetValue(obj)?.ToString();
                        if (val != null)
                        {
                            l.Href = MatchReplace(l.Href, match, val);
                        }
                    }
                }
            }

            return l;
        }

        private string MatchReplace(string str, Match match, string replace)
        {
            var capture = match.Captures[0];
            return str.Substring(0, capture.Index) + replace + str.Substring(capture.Index + capture.Length);
        }
    }

    class LinkWithMetadata
    {
        public Link Link { get; set; }

        // The callback should both check delegated admin permissions on the route
        // itself, and also call the "CanXXX" method related to the route handler,
        // if one is present.
        public CheckAvailabilityDelegate CheckAvailability { get; set; }

        // (Context, resource identifier parameters) => available / not available
        public delegate bool CheckAvailabilityDelegate(HttpContext context, IDictionary<string, string> resourceIdentifierParameters);
    }
}
