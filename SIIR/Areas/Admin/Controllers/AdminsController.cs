using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIIR.DataAccess.Data.Repository.IRepository;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminsController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;

        public AdminsController(IContenedorTrabajo contenedorTrabajo)
        {
            _contenedorTrabajo = contenedorTrabajo;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Admin.GetAll() });
        }
    }
}
