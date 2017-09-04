using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ApiFoundation.Rbac
{
    internal class CcAuthorizationHandler : AuthorizationHandler<CcAuthBearerTokenRequirement, CcAuthValidationContext>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CcAuthBearerTokenRequirement requirement,
            CcAuthValidationContext resource)
        {
            // build the exact required policies by filling in the required policy scopes
            // with the provided scope parameters


            // verify the user (in context.User) against the required policis


            // If successful -
            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}