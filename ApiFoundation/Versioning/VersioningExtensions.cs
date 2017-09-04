using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ApiFoundation.Versioning
{
    internal static class VersioningExtensions
    {
        public static void AddVersioningRoute(this MvcOptions opt, string versionPrefix)
        {
            if (!string.IsNullOrEmpty(versionPrefix))
            {
                var routeAttribute = new RouteAttribute(versionPrefix);
                opt.Conventions.Insert(0, new RouteConvention(routeAttribute));
            }
        }

        public static IServiceCollection AddVersioningConstraint(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
                options.ConstraintMap.Add("maxversion", typeof(ApiVersionRouteConstraint)));
            return services;
        }
    }
}