using Microsoft.AspNetCore.Mvc;
using SIIR.Data;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;

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

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Representative representative)
        {
            if (ModelState.IsValid)
            {
                _contenedorTrabajo.Representative.Add(representative);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(representative);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Representative representative = new Representative();
            representative = _contenedorTrabajo.Representative.GetById(id);
            if (representative == null)
            {
                return NotFound();
            }
            return View(representative);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Representative representative)
        {
            if (ModelState.IsValid)
            {
                _contenedorTrabajo.Representative.Update(representative);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(representative);
        }


        #region

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Representative.GetAll() });
        }

        [HttpDelete]
        public IActionResult Delete(int id) {
            var objFromDb = _contenedorTrabajo.Representative.GetById(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error al borrar el grupo representativo." });
            }
            _contenedorTrabajo.Representative.Remove(objFromDb);
            _contenedorTrabajo.Save();
            return Json(new { success = true, message = "Grupo representativo borrado exitosamente." });
        }

        #endregion

    }
}
