using Microsoft.AspNetCore.Mvc;

namespace Faceit_Stats_Provider.Controllers
{
    public class AnalyzerController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Analyzer/Analyzer.cshtml");
        }
    }
}
