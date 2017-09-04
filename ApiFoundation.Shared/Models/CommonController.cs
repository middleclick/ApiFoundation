using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiFoundation.Shared.Models
{
    /// <summary>
    /// Common base class for API controllers.
    /// </summary>
    /// <remarks>
    /// Plugins should extend all controllers from this class in order to gain
    /// full functionality from the API platform.
    /// </remarks>
    public class CommonController : Controller
    {
        // The name "ControllerBase" conflicts with the framework making usage awkward
    }
}