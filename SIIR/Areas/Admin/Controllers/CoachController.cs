using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public CoachController(
            IContenedorTrabajo contenedorTrabajo, 
            IWebHostEnvironment hostingEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _contenedorTrabajo = contenedorTrabajo;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
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

			// Obtener el usuario asociado al coach
			var user = _contenedorTrabajo.User.GetAll(u => u.CoachId == id).FirstOrDefault();

			// Guardar email y teléfono en ViewBag
			ViewBag.UserEmail = user?.Email ?? "No disponible";
			ViewBag.UserPhone = user?.PhoneNumber ?? "No disponible";

			return View(coach);
        }


        #region API_CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Coach.GetAll() });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = _contenedorTrabajo.Coach.GetById(id);
            var user = _contenedorTrabajo.User.GetAll(u => u.CoachId == id).FirstOrDefault();

            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error al borrar Coach"});
            }

            _contenedorTrabajo.Coach.Remove(objFromDb);

            // Delete Identity user if exists
            if (user != null)
            {
                var identityUser = await _userManager.FindByIdAsync(user.Id);
                if (identityUser != null)
                {
                    var result = await _userManager.DeleteAsync(identityUser);
                    if (!result.Succeeded)
                    {
                        return Json(new { success = false, message = "Error al borrar la cuenta del usuario." });
                    }
                }
            }

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
