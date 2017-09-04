using System.Security.Claims;

namespace ApiFoundation.Shared.Rbac
{
    /// <summary>
    /// Identity of a caller who used CCAuth
    /// </summary>
    public class CcIdentity : ClaimsIdentity
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CcIdentity(string name) : base() => _name = name;

        /// <summary>
        /// Type of authentication used; always CCBearer
        /// </summary>
        public override string AuthenticationType => CcBearerOptions.Scheme;

        /// <summary>
        /// Whether authenticated; always true
        /// </summary>
        /// <remarks>
        /// We don't create the CcIdentity object unless the caller is successfully authenticated.
        /// </remarks>
        public override bool IsAuthenticated => true;

        /// <summary>
        /// Human-readable name of the identity.
        /// </summary>
        public override string Name => _name;

        private readonly string _name;
    }
}