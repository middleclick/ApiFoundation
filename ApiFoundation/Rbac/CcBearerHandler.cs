using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ApiFoundation.Shared.Rbac;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ApiFoundation.Rbac
{
    internal class CcBearerHandler : AuthenticationHandler<CcBearerOptions>
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
            fakeidentity.AddClaims(new [] {
                // Identity claims, which come from the bearer token
                new Claim("sub", "fakesub"),
                new Claim(ClaimTypes.Name, "fake identity"),
                new Claim(ClaimTypes.Email, "fake.email@email.com"),
                new Claim("customers", JsonConvert.SerializeObject(token.Split(','))),
            });

            // TODO - we could (if we wanted to) get all of the roles
            // and scopes that the caller has access to, from all of
            // the products, and add those as claims as well.  However
            // it might result in poor performance since you'd have to
            // figure out all rights from all products on every API call.
            // The alternative is to check rights on demand instead of
            // up-front.

            var principal = new ClaimsPrincipal(fakeidentity);

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, CcBearerOptions.Scheme)));
        }
    }
}