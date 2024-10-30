using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;

namespace SIIR.Areas.Student.Controllers
{
	[Area("Student")]
    [Authorize(Roles = "Student")]
    public class HomeController : Controller
	{
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IContenedorTrabajo _contenedorTrabajo;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IContenedorTrabajo contenedorTrabajo)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _contenedorTrabajo = contenedorTrabajo;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.Users
                .Include(u => u.Student)
                    .ThenInclude(s => s.Team)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (currentUser?.Student == null)
            {
                return NotFound();
            }

            var student = currentUser.Student;
            var fullName = $"{student.Name} {student.LastName} {student.SecondLastName}".Trim();
            var imageUrl = !string.IsNullOrEmpty(student.ImageUrl)
                ? student.ImageUrl
                : "/profile.jpg"; // Imagen por defecto
            var teamName = student.Team?.Name ?? "No asignado";

            ViewData["StudentId"] = student.Id;
            ViewData["FullName"] = fullName;
            ViewData["ControlNumber"] = student.ControlNumber;
            ViewData["Semester"] = student.Semester;
            ViewData["Career"] = student.Career;
            ViewData["TeamName"] = teamName;
            ViewData["ImageUrl"] = imageUrl;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.Users
                .Include(u => u.Student)
                    .ThenInclude(s => s.Team)
                .Include(u => u.Student.Coach)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (currentUser?.Student == null)
            {
                return NotFound();
            }

            return View(currentUser.Student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Models.Student student)
        {
            // Verificar que el usuario actual es el dueño del perfil
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.Users
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (currentUser?.Student == null || currentUser.Student.Id != student.Id)
            {
                return Forbid();
            }

            // Quitar Coach y Team del ModelState
            ModelState.Remove("Coach");
            ModelState.Remove("Team");

            if (ModelState.IsValid)
            {
                string webRootPath = _webHostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                if (files.Count > 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\students");
                    var extension = Path.GetExtension(files[0].FileName);

                    // Eliminar la imagen anterior
                    if (!string.IsNullOrEmpty(student.ImageUrl))
                    {
                        var imagePath = Path.Combine(webRootPath, student.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    // Subir la nueva imagen
                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    student.ImageUrl = @"\images\students\" + fileName + extension;
                }
                else
                {
                    // Mantener la imagen actual
                    student.ImageUrl = currentUser.Student.ImageUrl;
                }

                // Mantener los IDs existentes
                student.TeamId = currentUser.Student.TeamId;
                student.CoachId = currentUser.Student.CoachId;

                _contenedorTrabajo.Student.Update(student);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }

            // Recargar las relaciones para la vista
            student.Team = currentUser.Student.Team;
            student.Coach = currentUser.Student.Coach;
            return View(student);
        }

        /*[HttpGet]
        public IActionResult GetAllUniform(int representativeId)
        {
            var representative = _contenedorTrabajo.Representative
                .GetAll(r => r.Id == representativeId, includeProperties: "UniformCatalogs")
                .FirstOrDefault();

            if (representative == null || representative.UniformCatalogs == null)
            {
                return Json(new { data = Array.Empty<object>() });
            }

            return Json(new { data = representative.UniformCatalogs.Select(u => new { u.Id, u.Name }) });
        }*/
    }
}
