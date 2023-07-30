using Microsoft.AspNetCore.Mvc;

namespace FYP.Controllers;

public class DefaultController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
