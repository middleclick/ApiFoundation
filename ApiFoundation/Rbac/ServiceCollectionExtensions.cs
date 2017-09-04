using ApiFoundation.Shared.Rbac;
using Microsoft.Extensions.DependencyInjection;

namespace ApiFoundation.Rbac
{
    internal static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCcAuth(this IServiceCollection services)
        {
            services.AddAuthentication(CcBearerOptions.Scheme)
                    .AddScheme<CcBearerOptions, CcBearerHandler>(CcBearerOptions.Scheme, _ => {});
            return services;
        }
    }
}