using Microsoft.AspNetCore.Mvc;

namespace SIIR.Areas.Student.Controllers
{
    [Area("Student")]
    public class InformationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
