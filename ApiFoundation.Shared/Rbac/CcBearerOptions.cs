using Microsoft.AspNetCore.Authentication;

namespace ApiFoundation.Shared.Rbac
{
    /// <summary>
    /// CcAuth global authentication options.
    /// </summary>
    public class CcBearerOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Name of the CCAuth scheme, used internally by ASP.NET MVC Core 2
        /// </summary>
        public const string Scheme = "CCBearer";
    }
}