using System;
using ApiFoundation.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace XaXd.Api
{
    /// <summary>
    /// Delivery groups APIs
    /// </summary>
    [Route("[controller]")]
    public class DeliveryGroupsController : CommonController
    {
        /// <summary>
        /// Get all delivery groups, returns all basic details on the main view in Studio.
        /// </summary>
        /// <requestHeader name="Authenticate" example="CWSAuth Bearer={bearer token}" required="true"></requestHeader>
        /// <requestHeader name="Accept" example="application/json">We only test &amp; support JSON payloads.</requestHeader>
        /// <requestHeader name="X-CC-Locale" example="en-US">Indicates the admin's preferred language and locale.  Any error responses that may be displayed to the admin, must be returned in the admin's locale.</requestHeader>
        /// <requestHeader name="X-Cws-TransactionId" example="{guid}">Indicates the overall transaction in which the call is being made.  The transaction ID is passed into all outbound requests so that support can follow the flow of an API call through all services.</requestHeader>
        /// <response code="401">The caller is unknown.  This response code is returned if the caller presents an invalid, expired, or revoked CC bearer token.</response>
        /// <response code="403">The caller is unauthorized.  This response code is returned if the caller (as identified by the CC bearer token) does not have permission to call the API.</response>
        /// <response code="429">The caller has made too many requests in a short period of time and is being rate-limited.</response>
        /// <response code="500">An unexpected exception occurred within the service while processing the API call. </response>
        /// <response code="503">The service is overloaded.  The caller should call the API again with identical HTTP verb and payload with an exponential back-off, until either the call succeeds or a set number of failures occurs.</response>
        /// <responseHeader name="Content-Type" example="application/json" present="always"></responseHeader>
        [HttpGet]
        public DeliveryGroupsCollection Get()
        {
            throw new NotImplementedException();
        }
    }

}