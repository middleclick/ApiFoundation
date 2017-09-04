using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiFoundation.Shared.Models;
using ApiFoundation.Shared.Rbac;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;

namespace ApiFoundation.ResourceLinking
{
    internal class RouteDictionary : Dictionary<string, IList<LinkWithMetadata>>
    {
        protected RouteDictionary() {}
        
        public static RouteDictionary Create(
            IActionDescriptorCollectionProvider provider,
            IServiceProvider serviceProvider)
        {
            var routeDict = new RouteDictionary();
            var routes = provider.ActionDescriptors.Items
                .Where(r => !string.IsNullOrEmpty(r.AttributeRouteInfo?.Template))
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
                MethodInfo canMethod = null;
                ParameterInfo[] canParameters = null;
                if (controllerInfo != null && methodInfo != null)
                {
                    canMethod = controllerInfo.GetMethod("Can" + methodInfo.Name);
                    if (canMethod?.ReturnType == typeof(bool))
                    {
                        canParameters = canMethod.GetParameters();
                    }
                    else
                    {
                        // bad method, ditch it.  It has to return a bool.
                        canMethod = null;
                    }
                }

                var route = new LinkWithMetadata { Link = link };
                route.CheckAvailability = (ctx, cntrl, rd) =>
                    RouteChecker.CheckAvailabilityCallback(
                        serviceProvider,
                        permissions, scopes?.Scopes, scopeParams,
                        canMethod, canParameters,
                        ctx, cntrl, rd);

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

        public static string GetRouteName(ActionDescriptor ad)
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

        public static string GetRouteMethod(ActionDescriptor ad)
        {
            var result = ad.ActionConstraints?.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods.FirstOrDefault();
            return result == "GET" ? null : result;
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
    }
}