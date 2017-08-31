using System.Reflection;
using ApiFoundation.ResourceLinking;
using ApiFoundation.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace ApiFoundation
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mvc = services.AddMvc(
                opt => {
                    opt.UseCentralRoutePrefix(new RouteAttribute("v1/{customer}"));
                    opt.Filters.Add(new ProducesAttribute("application/json"));
                    opt.Filters.Add<LinkFilter>();
                })
                .AddApplicationPart(Assembly.LoadFile(@"/Users/tomkludy/Projects/ApiFoundation/HelloWorld.Api/bin/Debug/netcoreapp2.0/HelloWorld.Api.dll"));

            services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
                    c.OperationFilter<ApiVersionOperationFilter>();
                });

            services.Configure<RouteOptions>(options =>
                options.ConstraintMap.Add("maxversion", typeof(ApiVersionRouteConstraint)));

            // This should be pointed to Redis
            services.AddDistributedMemoryCache();
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
}
