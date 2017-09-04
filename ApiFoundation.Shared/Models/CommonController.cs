using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiFoundation.Shared.Models
{
    //[Authorize]
    public class CommonController : Controller
    {
        // The name "ControllerBase" conflicts with the framework making usage awkward
    }
}