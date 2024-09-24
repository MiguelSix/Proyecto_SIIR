using Microsoft.AspNetCore.Mvc;

namespace SIIR.Areas.Student.Controllers
{
    public class InformationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
