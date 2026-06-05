using Microsoft.AspNetCore.Mvc;

namespace ACUEDUCTO2.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
