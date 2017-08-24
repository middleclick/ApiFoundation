using System;
using ServiceStack;
using ServiceStack.Api.OpenApi;
using ServiceStack.Caching;
using ServiceStack.Formats;
using ServiceStack.Text;

namespace ApiTest
{
    public class AppHost : AppHostBase {
        public AppHost() 
        : base("HttpListener Self-Host", typeof(HelloService).Assembly) { }

        public override void Configure(Funq.Container container) {

            SetConfig(new HostConfig{
                AllowJsonpRequests = false,
                ApiVersion = DateTime.Now.ToString("yyyy-MM-dd"),
                DefaultContentType = MimeTypes.Json,
            });

            JsConfig.EmitCamelCaseNames = true;

            Plugins.RemoveAll(x => x is CsvFormat);
            //Plugins.RemoveAll(x => x is HtmlFormat);
            Plugins.RemoveAll(x => x is MarkdownFormat);
            Plugins.Add(new OpenApiFeature {
                ApiDeclarationFilter = api => api.FixSwaggerVersionPath(),
                UseCamelCaseSchemaPropertyNames = true,
                OperationFilter = (s, api) => api.AddVersionInfo(),
            });

            container.Register<ICacheClient>(new MemoryCacheClient());
        }

        public override RouteAttribute[] GetRouteAttributes(System.Type requestType)
        {
            var routes = base.GetRouteAttributes(requestType);
            // Eventually, this could check routes[].GetDto() with reflection, to determine the right version.
            // Right now, there is only a v1 major version.
            routes.Each(x => x.Path = "/v1" + x.Path);
            return routes;
        }
    }
}
