using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIIR.Areas.Coach.Controllers
{
    [Area("Coach")]
    [Authorize(Roles = "Coach")]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
