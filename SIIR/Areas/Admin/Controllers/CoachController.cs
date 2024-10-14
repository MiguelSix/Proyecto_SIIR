using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CoachController : Controller
    {
        //Se utiliza el contenedor de trabajo de Coach para almacenar los datos y utilizarlos
        private readonly IContenedorTrabajo _contenedorTrabajo;

        public CoachController(IContenedorTrabajo contenedorTrabajo)
        {
            _contenedorTrabajo = contenedorTrabajo;
        }

        //Se utiliza HttpGet por que se van a traer y utilizar los datos del coach para acceso a vistas y asi?
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
        public IActionResult Create(Models.Coach coach)
        {
            if (ModelState.IsValid)
            {
                _contenedorTrabajo.Coach.Add(coach);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(coach);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Models.Coach coach = new Models.Coach();
            coach = _contenedorTrabajo.Coach.GetById(id);
            
            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.Coach coach)
        {
            if (ModelState.IsValid)
            {
                _contenedorTrabajo.Coach.Update(coach);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(coach);
        }


        #region Llamadas a la API

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Coach.GetAll() });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _contenedorTrabajo.Coach.GetById(id);

            //Obtener el id de usuario basado en el id de coach en la tabla Users

            var user = _contenedorTrabajo.User.GetAll(u => u.CoachId == id).FirstOrDefault();

            if (objFromDb == null)
            {
                return Json(new { succes = false, message = "Error al borrar Coach"});
            }

            _contenedorTrabajo.Coach.Remove(objFromDb);
            _contenedorTrabajo.User.Remove(user);
            _contenedorTrabajo.Save();
            return Json(new { succes = true, message = "Exito al borrar Coach" });
        }
        #endregion
    }
}
