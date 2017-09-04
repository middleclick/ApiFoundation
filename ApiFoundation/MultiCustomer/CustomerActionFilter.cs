using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace ApiFoundation.MultiCustomer
{
    public class CustomerActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do nothing.
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Determine which customer(s) the caller has access to
            var caller = context.HttpContext.User;
            var customersClaim = caller.FindFirst("customers");
            if (customersClaim == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            string[] customers;
            try
            {
                customers = JsonConvert.DeserializeObject<string[]>(customersClaim.Value);
            }
            catch
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (customers.Length == 0)
            {
                // No access to any customer
                context.Result = new ForbidResult();
                return;
            }

            string customer;

            // Check whether the caller has passed in a customer query parameter
            if (context.HttpContext?.Request?.Query != null &&
                context.HttpContext.Request.Query.TryGetValue("customer", out var customerParam))
            {
                customer = customers.FirstOrDefault(c => StringComparer.OrdinalIgnoreCase.Equals(c, customerParam));
                if (customer == null)
                {
                    // Caller asked for a customer context that they don't have access to.
                    context.Result = new ForbidResult();
                    return;
                }
            }
            else
            {
                if (customers.Length > 1)
                {
                    // They didn't tell us which customer to use, and they have access to multiple;
                    // we can't proceed without more information.
                    var errors = new ModelStateDictionary();
                    errors.AddModelError("customer", $"Customer is ambiguous; may be any of {string.Join(',', customers)}.  Specify the 'customer' query parameter to disambiguate.");
                    context.Result = new BadRequestObjectResult(errors);
                    return;
                }
                customer = customers[0];
            }

            context.ActionArguments["customer"] = customer;
        }
    }
}