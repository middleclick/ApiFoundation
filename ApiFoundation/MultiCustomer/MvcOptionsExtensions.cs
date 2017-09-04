using Microsoft.AspNetCore.Mvc;

namespace ApiFoundation.MultiCustomer
{
    internal static class MvcOptionsExtensions
    {
        public static void AddMultiCustomer(this MvcOptions opt)
        {
            opt.Filters.Add<CustomerActionFilter>();
        }
    }
}