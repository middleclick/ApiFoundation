using System;
using System.Globalization;
using System.Text.RegularExpressions;
using ApiFoundation.Shared.Models;
using Microsoft.AspNetCore.Http;

namespace ApiFoundation.Versioning
{
    public static class ApiVersionLinkFilter
    {
        private static Regex _maxVersionRex = new Regex(@":maxversion\((\d\d\d\d-\d\d-\d\d)\)", RegexOptions.Compiled);

        public static bool CheckLink(Link link, HttpContext context)
        {
            // If a link has a maxversion, see if the caller has access to it; skip otherwise
            var match = _maxVersionRex.Match(link.Href);
            if (match.Success)
            {
                var maxVersion = match.Groups[1].Value;
                DateTime date;
                if (!DateTime.TryParseExact(maxVersion, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    throw new ArgumentException("Invalid version format", nameof(maxVersion));

                if (!ApiVersionRouteConstraint.DoesRequestVersionMatch(context, date))
                    return false;

                // cut the maxversion out
                link.Href = MatchReplace(link.Href, match, "");
            }
            return true;
        }

        private static string MatchReplace(string str, Match match, string replace)
        {
            var capture = match.Captures[0];
            return str.Substring(0, capture.Index) + replace + str.Substring(capture.Index + capture.Length);
        }
    }
}