using Microsoft.AspNetCore.Mvc;

namespace Faceit_Stats_Provider.Controllers
{
    public class FAQController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/FAQ/FAQ.cshtml");
        }
    }
}
