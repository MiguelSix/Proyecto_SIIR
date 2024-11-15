using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Newtonsoft.Json;
using SIIR.Data;
using SIIR.DataAccess.Data.Repository;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using System.Security.Claims;

namespace SIIR.Areas.Coach.Controllers
{
    [Area("Coach")]
    [Authorize(Roles = "Coach")]
    public class HomeController : Controller
    {

        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public HomeController(
               IContenedorTrabajo contenedorTrabajo, 
               UserManager<ApplicationUser> userManager,
               IWebHostEnvironment hostingEnvironment)
        {
            _contenedorTrabajo = contenedorTrabajo;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public async Task <IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            { 
                return NotFound();
            }
            ApplicationUser user = await _userManager.FindByIdAsync(userId); 
            Models.Coach coach = new Models.Coach();
            if (user.CoachId == null)
            { 
                return NotFound();
            }
            coach = _contenedorTrabajo.Coach.GetById((int) user.CoachId);
            return View(coach);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return NotFound();
            }

            var coach = _contenedorTrabajo.Coach.GetById(id);
            if (coach == null)
            {
                return NotFound();
            }

            var user = _userManager.FindByIdAsync(userId).Result;
            if (user.CoachId != id)
            {
                return Forbid();
            }
            return View(coach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.Coach coach)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return NotFound();
            }

            var user = _userManager.FindByIdAsync(userId).Result;
            if (user.CoachId != coach.Id)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    var coachFromDb = _contenedorTrabajo.Coach.GetById(coach.Id);

                    if (coachFromDb == null)
                    {
                        return NotFound();
                    }

                    // Procesar la imagen
                    if (Request.Form.Files.Count > 0)
                    {
                        // Buscar el archivo de imagen por su nombre
                        var imageFile = Request.Form.Files["subidaArchivo"];
                        if (imageFile != null && imageFile.Length > 0)
                        {
                            var uploadsFolder = Path.Combine(webRootPath, "images", "coaches");
                            Directory.CreateDirectory(uploadsFolder);

                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                            string filePath = Path.Combine(uploadsFolder, fileName);

                            if (!string.IsNullOrEmpty(coachFromDb.ImageUrl))
                            {
                                var oldImagePath = Path.Combine(webRootPath, coachFromDb.ImageUrl.TrimStart('/'));
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                            }

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                imageFile.CopyTo(fileStream);
                            }

                            coachFromDb.ImageUrl = "/images/coaches/" + fileName;
                        }

                        // Buscar el archivo de CV por su nombre
                        var cvFile = Request.Form.Files["subidaCV"];
                        if (cvFile != null && cvFile.Length > 0)
                        {
                            var uploadsFolder = Path.Combine(webRootPath, "documents", "cv");
                            Directory.CreateDirectory(uploadsFolder);

                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(cvFile.FileName);
                            string filePath = Path.Combine(uploadsFolder, fileName);

                            if (!string.IsNullOrEmpty(coachFromDb.CVUrl))
                            {
                                var oldCVPath = Path.Combine(webRootPath, coachFromDb.CVUrl.TrimStart('/'));
                                if (System.IO.File.Exists(oldCVPath))
                                {
                                    System.IO.File.Delete(oldCVPath);
                                }
                            }

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                cvFile.CopyTo(fileStream);
                            }

                            coachFromDb.CVUrl = "/documents/cv/" + fileName;
                        }
                    }

                    // Actualizar los demás campos
                    coachFromDb.Name = coach.Name;
                    coachFromDb.LastName = coach.LastName;
                    coachFromDb.SecondLastName = coach.SecondLastName;

                    _contenedorTrabajo.Coach.Update(coachFromDb);
                    _contenedorTrabajo.Save();

                    TempData["Success"] = "Perfil actualizado correctamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ha ocurrido un error al actualizar el perfil: " + ex.Message);
                }
            }
            return View(coach);
        }

        #region API_CALLS

        [HttpGet]
        public IActionResult GetAllTeams(int coachId)
        {
            var equipos = _contenedorTrabajo.Team.GetAll(t =>
                t.CoachId == coachId,
                includeProperties: "Representative");

            return Json(new { data = equipos });
        }

        public IActionResult GetStudentsByTeamId(int teamId)
        {
            var users = _contenedorTrabajo.User.GetAll(u => u.LockoutEnd == null && u.StudentId != null).Select(u => u.StudentId).ToList();
            var students = _contenedorTrabajo.Student.GetAll(s => s.TeamId == teamId && users.Contains(s.Id));
            return Json(new { data = students });
        }

        #endregion
    }
}
