using Microsoft.AspNetCore.Authorization;

namespace ApiFoundation.Rbac
{
    internal class CcAuthBearerTokenRequirement : IAuthorizationRequirement
    {
        // Nothing needed.. this requirement effectively says "the caller must use a CCAuth bearer token".
        // Specifics about the rights required by the caller are encoded in the CcAuthValidationContext.
    }
}