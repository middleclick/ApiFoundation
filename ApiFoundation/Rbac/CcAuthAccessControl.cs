using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;

namespace ApiFoundation.Rbac
{
    internal static class CcAuthAccessControl
    {
        public static bool CheckAccess(ClaimsPrincipal user, IList<string> permissions, IList<string> scopes, IDictionary<string, string> scopeParams)
        {
            IList<string> concreteScopes = null;
            if (scopes != null && scopeParams != null)
            {
                // convert scope patterns (like "CC:c_[customer]:ANY:[instance]:ANY") into concrete scopes
                // (like "CC:c_acme:ANY:123:ANY") by looking for the patterns (e.g. "customer" and "instance")
                // in the routeData (scope param=>route param=>resource param)
                concreteScopes = scopes.Select(scope => FillScopeParams(scope, scopeParams)).ToList();
            }

            // TODO real CCAuth checks
            return true;
        }

        public static IEnumerable<string> ScopeParamsNeeded(IEnumerable<string> scopes)
        {
            foreach (var scope in scopes ?? Enumerable.Empty<string>())
            {
                var matches = _rex.Matches(scope);
                foreach (Match m in matches)
                {
                    yield return m.Groups[1].Value;
                }
            }
        }

        private static string FillScopeParams(string scope, IDictionary<string, string> scopeParams)
        {
            while(true)
            {
                var match = _rex.Match(scope);
                if (!match.Success)
                    break;
                var scopeParam = match.Groups[1].Value;
                var resourceParam = scopeParams[scopeParam];
                scope = _rex.Replace(scope, resourceParam?.ToString() ?? string.Empty, 1);
            }
            return scope;
        }

        private const string _pattern = @"\[([^\]+)\]";
        private static readonly Regex _rex = new Regex(_pattern, RegexOptions.Compiled);

    }
}