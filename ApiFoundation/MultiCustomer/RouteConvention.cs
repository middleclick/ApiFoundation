using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace ApiFoundation
{
    public class RouteConvention : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _centralPrefix;

        public RouteConvention(IRouteTemplateProvider routeTemplateProvider) =>
            _centralPrefix = new AttributeRouteModel(routeTemplateProvider);

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                var matchedSelectors = controller.Selectors.Where(x => x.AttributeRouteModel != null).ToList();
                matchedSelectors.ForEach(selectorModel => {
                    // If the controller has an absolute template (starts with /) then don't modify it
                    if (!selectorModel.AttributeRouteModel.IsAbsoluteTemplate)
                    {
                        selectorModel.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_centralPrefix,
                            selectorModel.AttributeRouteModel);
                    }
                });

                var unmatchedSelectors = controller.Selectors.Where(x => x.AttributeRouteModel == null).ToList();
                unmatchedSelectors.ForEach(selectorModel => {
                    selectorModel.AttributeRouteModel = _centralPrefix;
                });
            }
        }
    }
}