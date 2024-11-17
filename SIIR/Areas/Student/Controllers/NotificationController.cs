using Microsoft.AspNetCore.Mvc;

namespace SIIR.Areas.Student.Controllers
{
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
