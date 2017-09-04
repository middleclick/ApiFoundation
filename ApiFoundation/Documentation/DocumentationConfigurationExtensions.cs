using System.Collections.Generic;
using System.Linq;
using ApiFoundation.PluginFramework;
using ApiFoundation.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace ApiFoundation.Documentation
{
    internal static class DocumentationConfigurationExtensions
    {
        public static IApplicationBuilder AddAppDocumentation(this IApplicationBuilder app, string appName, string versionPrefix)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{appName} {versionPrefix}");
                });
            return app;
        }

        public static IServiceCollection AddDocumentation(this IServiceCollection services, string appName, string versionPrefix, IList<PluginInfo> plugins)
        {
            services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc(versionPrefix, new Info { Title = appName, Version = versionPrefix });
                    c.OperationFilter<ApiVersionOperationFilter>();
                    plugins.Where(p => p.XmlDocFile != null).ToList().ForEach(p => c.IncludeXmlComments(p.XmlDocFile));
                });
            return services;
        }
    }
}