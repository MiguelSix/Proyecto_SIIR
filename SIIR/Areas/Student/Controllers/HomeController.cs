using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;
using SIIR.Utilities;
using System.Security.Claims;

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

            // Datos para el capitan
            ViewData["TeamId"] = student.TeamId;
            ViewData["IsCaptain"] = student.IsCaptain;

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

            ViewBag.CareerOptions = Careers.CareerList
                .Select(c => new SelectListItem
                {
                    Text = c,
                    Value = c
                })
                .ToList();

            var currentUser = await _userManager.Users
                .Include(u => u.Student)
                .ThenInclude(s => s.Team)
                .Include(u => u.Student.Coach)
                .Include(r => r.Student.Team.Representative)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (currentUser?.Student == null)
            {
                return NotFound();
            }

            StudentUniformVM studentVM = new StudentUniformVM()
            {
                student = currentUser.Student,
                uniforms = _contenedorTrabajo.Uniform
				            .GetAll(u => u.StudentId == currentUser.Student.Id)
				            .ToList()
			};

            foreach (var uniform in studentVM.uniforms)
            {


                var uniformNames = _contenedorTrabajo.UniformCatalog
                    .GetAll(uc => uc.Id == uniform.UniformCatalogId)
                    .Select(uc => uc.Name)
                    .ToList();

                foreach (var name in uniformNames)
                {
                    if (name is not null)
                        studentVM.namesUniform.Add(name);
                }
            }

				ViewBag.SizeOptions = Enum.GetValues(typeof(Size)).Cast<Size>();

			return View(studentVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentUniformVM studentVM)
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

            if (currentUser?.Student == null || currentUser.Student.Id != studentVM.student?.Id)
            {
                return Forbid();
            }

            if (currentUser.Student.numberUniform != null && studentVM.student.numberUniform != null)
            { 
			    var numbersStudents = _contenedorTrabajo.Student
		            .GetAll(s => s.TeamId == studentVM.student.TeamId && s.Id != studentVM.student.Id)
		            .Select(s => s.numberUniform)
		            .ToList();

			    if (numbersStudents.Contains(studentVM.student.numberUniform))
			    {
				    ModelState.AddModelError("student.numberUniform", "Este número ya está en uso por otro jugador en el equipo.");
			    }
			}

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
                    if (!string.IsNullOrEmpty(studentVM.student.ImageUrl))
                    {
                        var imagePath = Path.Combine(webRootPath, studentVM.student.ImageUrl.TrimStart('\\'));
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

					studentVM.student.ImageUrl = @"\images\students\" + fileName + extension;
                }
                else
                {
					// Mantener la imagen actual
					studentVM.student.ImageUrl = currentUser.Student.ImageUrl;
                }

				/*Agregar el numero a cada uniforme y la talla */

				if (studentVM.uniforms != null)
				{
					foreach (var uniform in studentVM.uniforms)
					{
                        _contenedorTrabajo.Uniform.Update(uniform);
					}
				}

				_contenedorTrabajo.Student.Update(studentVM.student);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }

            studentVM.student.Coach = _contenedorTrabajo.Coach.GetById(studentVM.student.CoachId);
            studentVM.student.Team = _contenedorTrabajo.Team.GetById(studentVM.student.TeamId);

			return View(studentVM);
        }

        [HttpGet]
        public IActionResult Roster(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = _contenedorTrabajo.Team.GetById(id);
            if (team == null)
            {
                return NotFound();
            }

            // Verificar que el usuario actual es el capitan del equipo
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = _userManager.Users
                .Include(u => u.Student)
                .FirstOrDefault(u => u.Id == user.Id);
            if (currentUser == null) {
                return NotFound();
            }

            if (currentUser.Student.TeamId != id || !currentUser.Student.IsCaptain)
            {
                return Forbid();
            }

            // Get the userRole and put it in the ViewBag
            ViewBag.UserRole = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Coach") ? "Coach" : "Student";

            var users = _contenedorTrabajo.User.GetAll(u => u.LockoutEnd == null && u.StudentId != null).Select(u => u.StudentId).ToList();
            var students = _contenedorTrabajo.Student.GetAll(s => s.TeamId == id.Value && users.Contains(s.Id)).ToList();
            var captain = students.FirstOrDefault(s => s.IsCaptain);

            TeamVM teamVM = new()
            {
                Team = team,
                Captain = captain
            };

            team.Coach = _contenedorTrabajo.Coach.GetById(team.CoachId);
            return View(teamVM);
        }
    }
}
