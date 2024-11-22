using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;
using SIIR.Utilities;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentsController(
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
        public IActionResult Lock(int id)
        {
            var student = _contenedorTrabajo.Student.GetFirstOrDefault(s => s.Id == id);
            if(student.IsCaptain)
            {
                return Json(new { success = false, message = "No se puede dar de baja al capitán" });
            }
            var user = _contenedorTrabajo.User.GetFirstOrDefault(u => u.StudentId == id);
            if (user == null)
            {
                return NotFound();
            }
            _contenedorTrabajo.User.LockUser(user.Id);
            return Json(new { success = true, message = "Estudiante dado de baja con éxito" });
        }

        [HttpGet]
		[Authorize(Roles = "Admin, Coach")]
		public IActionResult Details(int id)
        {
            var student = _contenedorTrabajo.Student
                .GetFirstOrDefault(
                    s => s.Id == id,
                    includeProperties: "Team,Coach"
                );

			// Verificamos Team antes de acceder a sus propiedades
			if (student.Team != null)
			{
				var representative = _contenedorTrabajo.Representative
					.GetFirstOrDefault(r => r.Id == student.Team.RepresentativeId);
				if (representative != null)
				{
					student.Team.Representative = representative;
				}
			}

			// Obtener los uniformes del estudiante
			var uniforms = _contenedorTrabajo.Uniform
                .GetAll(u => u.StudentId == student.Id)
                .ToList();

            // Crear el ViewModel
            var studentVM = new StudentUniformVM
            {
                student = student,
                uniforms = uniforms,
                namesUniform = new List<string>(),
                availableTeams = _contenedorTrabajo.Team.GetListaTeams()
            };

            // Obtener los nombres de los uniformes desde el catálogo
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

            ViewBag.SizeOptions = Enum.GetValues(typeof(Models.Size)).Cast<Models.Size>();

            return View(studentVM);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Student.GetAll(includeProperties: "Team,Coach") });
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var objFromDb = _contenedorTrabajo.Student.GetById(id);
                if (objFromDb == null)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado." });
                }

                // Find the user associated with the student
                var user = _contenedorTrabajo.User.GetAll(u => u.StudentId == id).FirstOrDefault();

                // Delete related notifications first
                var notifications = _contenedorTrabajo.Notification.GetAll(n => n.StudentId == id).ToList();
                foreach (var notification in notifications)
                {
                    _contenedorTrabajo.Notification.Remove(notification);
                }

                // Handle image deletion safely
                if (!string.IsNullOrEmpty(objFromDb.ImageUrl))
                {
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    var imagePath = Path.Combine(webRootPath, objFromDb.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Delete Identity user if exists
                if (user != null)
                {
                    var identityUser = await _userManager.FindByIdAsync(user.Id);
                    if (identityUser != null)
                    {
                        var result = await _userManager.DeleteAsync(identityUser);
                        if (!result.Succeeded)
                        {
                            return Json(new { success = false, message = "Error al borrar la cuenta." });
                        }
                    }
                }

                // Remove student from repository
                _contenedorTrabajo.Student.Remove(objFromDb);
                _contenedorTrabajo.Save();

                return Json(new { success = true, message = "Estudiante borrado exitosamente." });
            }
            catch (Exception ex)
            {
                // Log the full exception details
                return Json(new { success = false, message = $"Error al borrar: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult GetCareers()
        {
            var careers = Careers.CareerList;

            // Return only the careers that have students in the database
            var studentsCareers = _contenedorTrabajo.Student.GetAll().Select(s => s.Career).Distinct();
            careers = careers.Where(c => studentsCareers.Contains(c)).ToList();

            return Json(careers);
        }

        private void ChangeUniforms(int studentId, Team team)
        {
            var representativeUniformCatalog = _contenedorTrabajo.RepresentativeUniformCatalog.GetAll(ruc => ruc.RepresentativeId == team.RepresentativeId);

            var deleteUniforms = _contenedorTrabajo.Uniform.GetAll(u => u.StudentId == studentId);

            foreach (var deleteUniform in deleteUniforms)
            {
                _contenedorTrabajo.Uniform.Remove(deleteUniform);
            }

            foreach (var ruc in representativeUniformCatalog)
            {
                var uniform = new Uniform();
                uniform.StudentId = studentId;
                uniform.RepresentativeId = ruc.RepresentativeId;
                uniform.UniformCatalogId = ruc.UniformCatalogId;
                _contenedorTrabajo.Uniform.Add(uniform);
            }
            _contenedorTrabajo.Save();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult ChangeTeam(int studentId, int teamId)
        {
            try
            {
                var student = _contenedorTrabajo.Student.GetById(studentId);
                if (student.TeamId == teamId)
                {
                    return Json(new { success = false, message = "El estudiante ya se encuentra en este equipo" });
                }

                var team = _contenedorTrabajo.Team.GetById(teamId);
                if (team == null)
                {
                    return Json(new { success = false, message = "Equipo no encontrado." });
                }

                if (student.IsCaptain)
                {
                    return Json(new { success = false, message = "No se puede cambiar de equipo al capitán. Primero cambie de capitán en el equipo actual" });
                }

                student.TeamId = teamId;
                student.numberUniform = null;
                _contenedorTrabajo.Student.Update(student);
                ChangeUniforms(student.Id, team);

                _contenedorTrabajo.Save();

                return Json(new { success = true, message = "Equipo actualizado exitosamente." });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar el equipo: " + ex.Message });
            }
        }

        [HttpPost]
        private static void CreateStudentCell(IContainer container, Models.Student student, Models.Coach coach, Models.Team team)
        {
            string imageUrl = student.ImageUrl != null && student.ImageUrl.StartsWith("/")
                ? student.ImageUrl.Substring(1)
                : student.ImageUrl ?? "";

            imageUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('\\'));

            if (!System.IO.File.Exists(imageUrl))
            {
                imageUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "zorro_default.png");
            }

            byte[] imageBytes = Array.Empty<byte>();

            imageBytes = System.IO.File.ReadAllBytes(imageUrl);

            container.Padding(2)
                .Border(1)
                .BorderColor(Colors.Black)
                .Background(Colors.Grey.Lighten4)
                .DefaultTextStyle(x => x.FontSize(14).LineHeight(1.5f))
                .Column(column =>
                {
                    // Row for Image and Name
                    column.Item().Padding(5).Row(row =>
                    {
                        // Centering the image and name
                        row.RelativeItem().Column(innerColumn =>
                        {
                            innerColumn.Item()
                                .AlignMiddle()
                                .AlignCenter()
                                .Width(140) // Define the width of the image
                                .Height(140) // Define the height of the image
                                .Image(imageBytes);

                            // Name above and below the image
                            innerColumn.Item().Text($"{student.Name ?? "Sin Actualizar"} {student.LastName ?? "Sin Actualizar"} {student.SecondLastName ?? "Sin Actualizar"}").Bold().FontSize(18).AlignCenter();
                        });
                    });

                    // Two-column layout for the rest of the information
                    column.Item().PaddingLeft(10).PaddingBottom(5).Row(row =>
                    {
                        row.RelativeItem().Column(leftColumn =>
                        {
                            leftColumn.Item().Text("Número de control").Bold();
                            leftColumn.Item().Text(student.ControlNumber ?? "Sin Actualizar");

                            leftColumn.Item().Text("Carrera").Bold();
                            leftColumn.Item().Text(student.Career ?? "Sin Actualizar");

                            leftColumn.Item().Text("CURP").Bold();
                            leftColumn.Item().Text(student.Curp ?? "Sin Actualizar");

                            leftColumn.Item().Text("Fecha de Nacimiento").Bold();
                            leftColumn.Item().Text(student.BirthDate ?? "Sin Actualizar");

                            leftColumn.Item().Text("Teléfono").Bold();
                            leftColumn.Item().Text(student.Phone ?? "Sin Actualizar");

                            leftColumn.Item().Text("NSS").Bold();
                            leftColumn.Item().Text(student.Nss ?? "Sin Actualizar");

                            leftColumn.Item().Text("Número de Jugador").Bold();
                            leftColumn.Item().Text(student.numberUniform?.ToString() ?? "Sin Actualizar");

                            leftColumn.Item().Text("Fecha de Ingreso").Bold();
                            leftColumn.Item().Text(student.enrollmentData?.ToString() ?? "Sin Actualizar");
                        });

                        row.RelativeItem().Column(rightColumn =>
                        {
                            rightColumn.Item().Text("Edad").Bold();
                            rightColumn.Item().Text(student.Age?.ToString() ?? "Sin Actualizar");

                            rightColumn.Item().Text("Tipo de Sangre").Bold();
                            rightColumn.Item().Text(student.BloodType ?? "Sin Actualizar");

                            rightColumn.Item().Text("Peso").Bold();
                            rightColumn.Item().Text(student.Weight ?? "Sin Actualizar");

                            rightColumn.Item().Text("Altura").Bold();
                            rightColumn.Item().Text(student.Height ?? "Sin Actualizar");

                            rightColumn.Item().Text("Alergias").Bold();
                            rightColumn.Item().Text(student.Allergies ?? "Sin Actualizar");

                            rightColumn.Item().Text("Entrenador").Bold();
                            rightColumn.Item().Text(coach.Name ?? "Sin Actualizar");

                            rightColumn.Item().Text("Equipo").Bold();
                            rightColumn.Item().Text(team.Name ?? "Sin Actualizar");

                            rightColumn.Item().Text("Capitán").Bold();
                            rightColumn.Item().Text(student.IsCaptain ? "Sí" : "No");
                        });
                    });
                });
        }

        [HttpPost]
        public IActionResult GenerateStudentCertificate(int id)
        {
            // Obtener el estudiante por su ID
            var student = _contenedorTrabajo.Student.GetById(id);
            if (student == null)
            {
                return NotFound("Estudiante no encontrado");
            }

            var team = _contenedorTrabajo.Team.GetById(student.TeamId);
            var coach = _contenedorTrabajo.Coach.GetById(team.CoachId);

            // Formato de fecha
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            var studentName = student.Name.Replace(" ", "_"); // Reemplaza espacios por guiones bajos
            var studentControlNumber = student.ControlNumber;
            var fileName = $"Informacion_{studentName}_{studentControlNumber}_{date}.pdf";

            // Crear el documento PDF usando QuestPDF
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Content().Element(c => CreateStudentCell(c, student, coach, team));
                    page.Footer().Text(text => text.CurrentPageNumber());
                });
            });

            byte[] pdfBytes = document.GeneratePdf();

            // Configurar el encabezado Content-Disposition con el nombre personalizado
            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            return File(pdfBytes, "application/pdf");
        }

        #endregion
    }
}
