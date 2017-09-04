using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ApiFoundation.Rbac
{
    public class CcBearerOptions : AuthenticationSchemeOptions
    {
        public const string Scheme = "CWSAuth";
    }

    public class CcBearerHandler : AuthenticationHandler<CcBearerOptions>
    {
        public CcBearerHandler(IOptionsMonitor<CcBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authorization = Request.Headers["Authorization"];
            string authPrefix = "CWSAuth bearer=";
            string token = null;

            // If no authorization header found, nothing to process further
            if (string.IsNullOrEmpty(authorization))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (authorization.StartsWith(authPrefix, StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring(authPrefix.Length).Trim();
            }

            // If no token found, no further work possible
            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // TODO - validate the token (by calling CCAuth library)
            // And get all of the claims in it...
            // use either AuthenticationResult.Fail(); or...

            var fakeidentity = new CcIdentity("fake identity");
            var claimsIdentity = new ClaimsIdentity(fakeidentity, new [] {
                // Identity claims, which come from the bearer token
                new Claim("sub", "fakesub"),
                new Claim("name", "fake identity"),
                new Claim("email", "fake.email@email.com"),
                new Claim("customers", JsonConvert.SerializeObject(token.Split(','))),
            });

            // TODO - we could (if we wanted to) get all of the roles
            // and scopes that the caller has access to, from all of
            // the products, and add those as claims as well.  However
            // it might result in poor performance since you'd have to
            // figure out all rights from all products on every API call.
            // The alternative is to check rights on demand instead of
            // up-front.

            var principal = new ClaimsPrincipal(claimsIdentity);

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, CcBearerOptions.Scheme)));
        }
    }

    public class CcIdentity : ClaimsIdentity
    {
        public CcIdentity(string name) : base() => _name = name;

        public override string AuthenticationType => CcBearerOptions.Scheme;

        public override bool IsAuthenticated => true;

        public override string Name => _name;

        private readonly string _name;
    }

    public class CcAuthorizationHandler : AuthorizationHandler<CcAuthBearerTokenRequirement, CcAuthValidationContext>
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

    public class CcAuthBearerTokenRequirement : IAuthorizationRequirement
    {
        // Nothing needed.. this requirement effectively says "the caller must use a CCAuth bearer token".
        // Specifics about the rights required by the caller are encoded in the CcAuthValidationContext.
    }

    public class CcAuthValidationContext
    {
        public IList<CcAuthRequiredPolicy> RequiredPolicies { get; set; }
        public IDictionary<string, string> ScopeParameters { get; set; }
    }

    public class CcAuthRequiredPolicy
    {
        public IList<string> Policies { get; set; }
        public IList<string> ScopeTemplates { get; set; }
    }
}