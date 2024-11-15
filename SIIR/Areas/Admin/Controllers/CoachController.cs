using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CoachController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public CoachController(IContenedorTrabajo contenedorTrabajo, IWebHostEnvironment hostingEnvironment)
        {
            _contenedorTrabajo = contenedorTrabajo;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var coach = _contenedorTrabajo.Coach.GetById(id);
            if (coach == null)
            {
                return NotFound();
            }
            return View(coach);
        }


        #region API_CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Coach.GetAll() });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _contenedorTrabajo.Coach.GetById(id);

            var user = _contenedorTrabajo.User.GetAll(u => u.CoachId == id).FirstOrDefault();

            if (user != null)
            {
                _contenedorTrabajo.User.Remove(user);
            }

            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error al borrar Coach"});
            }

            _contenedorTrabajo.Coach.Remove(objFromDb);

            try
            {
                _contenedorTrabajo.Save();
                return Json(new { success = true, message = "Coach borrado correctamente" });
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Message.Contains("REFERENCE constraint"))
                {
                    return Json(new { success = false, message = "No se puede borrar el coach porque tiene un equipo asignado." });
                }
                return Json(new { success = false, message = "Ocurrió un error al borrar el coach." });
            }
        }
        #endregion
    }
}
