using Microsoft.AspNetCore.Mvc;
using SIIR.Data;
using SIIR.DataAccess.Data.Repository.IRepository;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RepresentativesController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;

        public RepresentativesController(IContenedorTrabajo contenedorTrabajo)
        {
            _contenedorTrabajo = contenedorTrabajo;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        #region

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Representative.GetAll() });
        }

        #endregion

    }
}
