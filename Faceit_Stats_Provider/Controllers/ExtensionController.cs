using Microsoft.AspNetCore.Mvc;

namespace Faceit_Stats_Provider.Controllers
{
    public class ExtensionController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Extension/Extension.cshtml");
        }
    }
}
