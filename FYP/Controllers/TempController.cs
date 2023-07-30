using Microsoft.AspNetCore.Mvc;

namespace FYP.Controllers
{
    public class TempController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
