using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Host.Handlers;

namespace ApiTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://0.0.0.0:5000/")
                .Build();
            
            host.Run();
        }
    }

    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseServiceStack(new AppHost());
            app.Use(new RequestInfoHandler());
        }
    }
}
