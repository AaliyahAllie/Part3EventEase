using Microsoft.AspNetCore.Mvc;

namespace EventEasePart3.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
