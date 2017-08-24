using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace ApiFoundation
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mvc = services.AddMvc(
                opt => {
                    opt.UseCentralRoutePrefix(new RouteAttribute("v1/{customer}"));
                    opt.Filters.Add(new ProducesAttribute("application/json"));
                    opt.Filters.Add<LinkFilter>();
                });

            services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
                    c.OperationFilter<ApiVersionOperationFilter>();
                });

            services.Configure<RouteOptions>(options =>
                options.ConstraintMap.Add("maxversion", typeof(ApiVersionRouteConstraint)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseSwagger();

            app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                });
        }
    }

    internal class LinkFilter : IResultFilter
    {
        public LinkFilter(IActionDescriptorCollectionProvider provider)
        {
            ActionProvider = provider;

            var routes = provider.ActionDescriptors.Items
                .Select(i => i.AttributeRouteInfo?.Template)
                .Where(r => !string.IsNullOrEmpty(r))
                .ToList();
            
            // Map from a route, to all related routes
            var routeDict = new Dictionary<string, IList<string>>();
        }

        public IActionDescriptorCollectionProvider ActionProvider { get; }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            // Can't do much here.
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            var objectResult = context.Result as ObjectResult;
            if (objectResult == null)
                return;
            
            var obj = objectResult.Value as LinkedResponse;
            if (obj == null || obj.Links != null)
                return;
            
            // Find all routes at the same route level, or one deeper


            obj.Links = new List<Link>();
        }
    }
}
