using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ApiFoundation.Rbac;
using ApiFoundation.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace ApiFoundation.ResourceLinking
{
    internal static class RouteChecker
    {
        public static bool CheckAvailabilityCallback(
            IServiceProvider serviceProvider,
            IList<string> permissions,
            IList<string> scopes,
            IDictionary<string, string> scopeParams, // (scope parameter => route parameter)
            MethodInfo canMethod,
            ParameterInfo[] canParameters,
            HttpContext context,
            Controller controller,
            RouteData routeData)
        {
            // Do CCAuth checks, if any
            // var scp = scopeParams.ToDictionary(kvp => kvp.Key, kvp => routeData.Values[kvp.Value]?.ToString() ?? string.Empty);
            var scopeValues =
                CcAuthAccessControl.ScopeParamsNeeded(scopes)
                    // The parameters may be returned more than once, but we only
                    // need to handle each name one time.
                    .Distinct()

                    // Find the route parameter names matching each scope name.
                    // If there isn't a mapping, then look up the route parameter
                    // by the scope name itself.
                    .Select(s => {
                        if (scopeParams != null && scopeParams.TryGetValue(s, out var r))
                        {
                            return (scopeName: s, routeParam: r);
                        }
                        else
                        {
                            return (scopeName: s, routeParam: s);
                        }
                    })

                    // Now find the route parameter values for each scope name,
                    // based on the route parameter names found above.
                    .Select(s => {
                        if (routeData?.Values != null && routeData.Values.TryGetValue(s.routeParam, out var v))
                        {
                            return (scopeName: s.scopeName, value: v?.ToString() ?? string.Empty);
                        }
                        else
                        {
                            return (scopeName: s.scopeName, value: string.Empty);
                        }
                    })

                    // Build a dictionary from scope parameter name => value.
                    .ToDictionary(s => s.scopeName, s => s.value);

            // Check CCAuth access.
            if (!CcAuthAccessControl.CheckAccess(context.User, permissions, scopes, scopeValues))
                return false;
            
            // Do method callback checks, if any
            if (canMethod != null && controller != null)
            {
                // Need to figure out how to do this.

                // This does not work...it blows up because ASP.NET already has a response, yet
                // tries to overwrite it with another response.

                // var af = serviceProvider.GetService(typeof(IActionInvokerFactory)) as ActionInvokerFactory;
                // var ac = new ActionContext(context, routeData, canAction);
                // var ai = af?.CreateInvoker(ac);
                // var t = ai?.InvokeAsync();
                // t?.Wait();                

                // Brute force:
                // * If a method parameter is in the routeData, use it.
                // * Else, if a method parameter type can be resolved from DI, do it.
                // * Else, blow up (don't include the link)
                object[] parmArray = null;
                if (canParameters != null)
                {
                    var paramValues = new List<object>();
                    foreach (var parm in canParameters)
                    {
                        object value;
                        if (routeData.Values.TryGetValue(parm.Name, out value))
                        {
                            paramValues.Add(value);
                        }
                        else if (context.Items.TryGetValue(parm.Name, out value))
                        {
                            paramValues.Add(value);
                        }
                        else if ((value = serviceProvider.GetService(parm.ParameterType)) != null)
                        {
                            paramValues.Add(value);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    parmArray = paramValues.ToArray();
                }

                var result = canMethod.Invoke(controller, parmArray);
                if (result is bool)
                    return (bool)result;
                return false;
            }

            return true;
        }
    }
}