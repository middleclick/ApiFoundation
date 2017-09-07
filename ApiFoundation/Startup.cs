using System.IO;
using System.Reflection;
using ApiFoundation;
using ApiFoundation.Documentation;
using ApiFoundation.MultiCustomer;
using ApiFoundation.PluginFramework;
using ApiFoundation.Rbac;
using ApiFoundation.ResourceLinking;
using ApiFoundation.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace ApiFoundation
{
    internal class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public const string ApplicationName = "Citrix Cloud API";
        public const string VersionPrefix = "v1";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Authentication
            services.AddCcAuth();

            // TODO: Authorization

            // Caching
            // TODO: This should be pointed to Redis
            services.AddDistributedMemoryCache();

            // Routing
            var mvc = services.AddMvc(
                opt => {
                    // Content negotiation
                    opt.Filters.Add(new ProducesAttribute("application/json"));

                    // Multi-customer
                    opt.AddMultiCustomer();

                    // Versioning.  Has to be added here and later.
                    opt.AddVersioningRoute(VersionPrefix);

                    // Linking (HATEOAS)
                    opt.Filters.Add<LinkFilter>();
                });
            
            // Versioning.  Has to be added after AddMvc.
            services.AddVersioningConstraint();

            // Plugin framework
            var plugins = mvc.AddPlugins();

            // Documentation
            services.AddDocumentation(ApplicationName, VersionPrefix, plugins);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.AddAppDocumentation(ApplicationName, VersionPrefix);

            // Must be the the last thing in this method.
            app.UseMvc();
        }
    }
}
