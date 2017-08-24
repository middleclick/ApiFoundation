using System;
using System.Linq;
using ServiceStack;
using ServiceStack.Api.OpenApi.Specification;

namespace ApiTest
{
    internal static class SwaggerExtensions
    {
        //
        // Some of the default doc generation behavior doesn't work well when
        // the version is enforced to be the first thing in the path.  This
        // function fixes that up, by setting the "Base URL" to include the
        // version, and then removing the version from all of the paths and
        // function doc categories.
        //
        internal static OpenApiDeclaration FixSwaggerVersionPath(this OpenApiDeclaration api)
        {
            api.BasePath = "/v1";
            var allItems = api.Paths.ToArray();
            api.Paths.Clear();
            foreach (var item in allItems)
            {
                var path = item.Key.StartsWith("/v1/") ? item.Key.Substring(3) : item.Key;
                var op = item.Value;
                var tags = path.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
                if (tags.Length > 0)
                {
                    // remove the "/v1" tag and instead, tag with the next part of the path
                    op.Delete?.Tags.Remove("v1");
                    op.Get?.Tags.Remove("v1");
                    op.Head?.Tags.Remove("v1");
                    op.Options?.Tags.Remove("v1");
                    op.Patch?.Tags.Remove("v1");
                    op.Post?.Tags.Remove("v1");
                    op.Put?.Tags.Remove("v1");

                    op.Delete?.Tags.Add(tags[0]);
                    op.Get?.Tags.Add(tags[0]);
                    op.Head?.Tags.Add(tags[0]);
                    op.Options?.Tags.Add(tags[0]);
                    op.Patch?.Tags.Add(tags[0]);
                    op.Post?.Tags.Add(tags[0]);
                    op.Put?.Tags.Add(tags[0]);
                }
                api.Paths.Add(path, op);
            }
            return api;
        }

        internal static OpenApiOperation AddVersionInfo(this OpenApiOperation api)
        {
            var obj = Type.GetType(api.RequestType);
            var ver = obj?.GetType().GetCustomAttributes(typeof(ApiVersionAttribute), true)
                         ?.OfType<ApiVersionAttribute>()
                         .FirstOrDefault();
            if (ver != null)
                api.Description += "\n\nAvailable since: " + ver.IntroductionDate;
            return api;
        }
    }
}