using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiFoundation
{
    public class ApiVersionOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var version = context.ApiDescription.ActionAttributes().OfType<ApiVersionAttribute>()
                .Concat(context.ApiDescription.ControllerAttributes().OfType<ApiVersionAttribute>())
                .FirstOrDefault();
            if (version != null)
            {
                operation.Description += $"\n\n<small>Introduced: {version.IntroductionDate}</small>";
            }
        }
    }
}
