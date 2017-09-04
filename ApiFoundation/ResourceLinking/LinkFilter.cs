using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
                _routeDict = new Lazy<IDictionary<string, IList<LinkWithMetadata>>>(
                    () => {
                        var routeDict = new Dictionary<string, IList<LinkWithMetadata>>();
                        var routes = provider.ActionDescriptors.Items
                            .Where(r => !string.IsNullOrEmpty(r.AttributeRouteInfo?.Template))
                            //.Select(x => new LinkWithMetadata { Link = new Link(GetRouteName(x), x.AttributeRouteInfo.Template, GetRouteMethod(x)) } )
                            .ToList();

                        // Map from a route, to all related routes
                        foreach (var x in routes)
                        {
                            var name = GetRouteName(x);
                            var href = x.AttributeRouteInfo.Template;
                            var method = GetRouteMethod(x);
                            var link = new Link(name, href, method);
                            
                            // Two ways to handle route availability:
                            // 1. by permissions and scopes; the latter may be parameterized by method properties
                            var cad = x as ControllerActionDescriptor;
                            var controllerInfo = cad?.ControllerTypeInfo;
                            var methodInfo = cad?.MethodInfo;
                            var attrs = methodInfo?.CustomAttributes;
                            var permissionAttr = attrs?.OfType<RbacPermissionAttribute>().FirstOrDefault();
                            var permissions = permissionAttr?.Permissions;
                            var scopes = permissions == null ? null : attrs?.OfType<RbacScopeAttribute>().FirstOrDefault();
                            var scopeParams = scopes == null ? null :
                                (from parm in (methodInfo?.GetParameters() ?? Enumerable.Empty<ParameterInfo>())
                                let scopeAttr = parm.GetCustomAttribute<RbacScopeParamAttribute>()
                                where scopeAttr != null
                                select (scopeAttr.ScopeParam, parm.Name))
                                .ToDictionary(t => t.Item1, t => t.Item2, StringComparer.OrdinalIgnoreCase);

                            // 2. by "CanXXX" method in the same controller, which must have
                            //    parameters that can be resolved from either the route parameters
                            //    or from the DI container
                            MethodInfo canMethodInfo = null;
                            if (methodInfo != null && controllerInfo != null)
                            {
                                canMethodInfo = controllerInfo.GetMethod("Can" + methodInfo.Name);
                            }

                            
                            
                            var route = new LinkWithMetadata { Link = link };
                            AddRouteLink(routeDict, href, route);
                            
                            var lastSep = href.LastIndexOf('/');
                            if (lastSep > 0)
                            {
                                var parent = href.Substring(0, lastSep);
                                AddRouteLink(routeDict, parent, route);
                            }
                        }
                        return routeDict;
                    }
                );
            }
        }

        private static bool CheckAvailabilityCallback(
            IServiceProvider serviceProvider,
            IList<string> permissions,
            IList<string> scopes,
            IDictionary<string, string> scopeParams, // (scope parameter => route parameter)
            MethodInfo canMethodInfo,
            HttpContext context,
            Link link,
            IDictionary<string, object> resourceIdentifierParameters)
        {
            IList<string> concreteScopes = null;
            if (scopes != null && scopeParams != null)
            {
                // convert scope patterns (like "CC:c_[customer]:ANY:[instance]:ANY") into concrete scopes
                // (like "CC:c_acme:ANY:123:ANY") by looking for the patterns (e.g. "customer" and "instance")
                // in the resourceIdentifierParameters (scope param=>route param=>resource param)
                concreteScopes = scopes.Select(scope => FillScopeParams(scope, scopeParams, resourceIdentifierParameters)).ToList();
            }

            return true;
        }

        private static string FillScopeParams(string scope, IDictionary<string, string> scopeParams, IDictionary<string, object> resourceIdentifierParameters)
        {
            var pattern = @"\[([^\]+)\]";
            var rex = new Regex(pattern);
            while(true)
            {
                var match = rex.Match(scope);
                if (!match.Success)
                    break;
                var scopeParam = match.Groups[1].Value;
                var resourceParam = scopeParams[scopeParam];
                var value = resourceParam == null ? null : resourceIdentifierParameters[resourceParam];
                scope = rex.Replace(scope, value?.ToString() ?? string.Empty, 1);
            }
            return scope;
        }

        private static void AddRouteLink(Dictionary<string, IList<LinkWithMetadata>> routeDict, string route, LinkWithMetadata link)
        {
            IList<LinkWithMetadata> segRoutes;
            if (!routeDict.TryGetValue(route, out segRoutes))
            {
                segRoutes = new List<LinkWithMetadata>();
                routeDict.Add(route, segRoutes);
            }
            segRoutes.Add(link);
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

        private static readonly string _parameterizedPattern = @"\{([^\}:]+)(:[^\}]+)?\}";
        private static readonly Regex _parameterized = new Regex(_parameterizedPattern, RegexOptions.Compiled);

        public void OnResultExecuting(ResultExecutingContext context)
        {
            var objectResult = context.Result as ObjectResult;
            if (objectResult == null)
                return;

            var obj = objectResult.Value as LinkedResponse;
            if (obj == null)
                return;

            var routeData = (context.Controller as Controller)?.RouteData?.Values;

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
                            if (IsRouteAvailableToCaller(route, ref subLink, context.HttpContext, routeData, item))
                            {
                                item.Links.Add(subLink);
                            }
                        }
                        continue;
                    }
                }
                
                // Note that this may modify the route link
                if (IsRouteAvailableToCaller(route, ref routeLink, context.HttpContext, routeData, obj))
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

        private bool IsRouteAvailableToCaller(LinkWithMetadata route, ref Link link, HttpContext httpContext, IDictionary<string, object> routeData, object objectContext)
        {
            var routeParams = new Dictionary<string, object>(routeData, StringComparer.OrdinalIgnoreCase);

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
                !route.CheckAvailability(httpContext, route.Link, routeParams))
                return false;

            return true;
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
    }

    class LinkWithMetadata
    {
        public Link Link { get; set; }

        // The callback should both check delegated admin permissions on the route
        // itself, and also call the "CanXXX" method related to the route handler,
        // if one is present.
        public CheckAvailabilityDelegate CheckAvailability { get; set; }

        // (Context, link, resource identifier parameters) => available / not available
        public delegate bool CheckAvailabilityDelegate(HttpContext context, Link link, IDictionary<string, object> resourceIdentifierParameters);
    }
}
