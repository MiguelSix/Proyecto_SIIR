using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TeamsController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public TeamsController(IContenedorTrabajo contenedorTrabajo,
            IWebHostEnvironment hostingEnvironment)
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
        public IActionResult Create()
        {
            TeamVM teamVM = new()
            {
                Team = new(),
                RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList(),
                CoachList = _contenedorTrabajo.Coach.GetCoachesList(),
            };
            return View(teamVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TeamVM teamVM)
        {
            if (!ModelState.IsValid)
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (teamVM.Team.Id == 0 && files.Count() > 0)
                {
                    // Nuevo equipo
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\teams");
                    var extension = Path.GetExtension(files[0].FileName);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStreams);
                    }

                    teamVM.Team.ImageUrl = @"\images\teams\" + fileName + extension;
                    _contenedorTrabajo.Team.Add(teamVM.Team);
                    _contenedorTrabajo.Save();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("Imagen", "Debes seleccionar una imagen");
                }
            }
            teamVM.RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList();
            teamVM.CoachList = _contenedorTrabajo.Coach.GetCoachesList();

			return View(teamVM);
        }


        [HttpGet]
        public IActionResult Edit(int? id)
        {
            TeamVM teamVM = new()
            {
                Team = new(),
                RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList(),
                CoachList = _contenedorTrabajo.Coach.GetCoachesList(),
            };
            if (id != null)
            {
                teamVM.Team = _contenedorTrabajo.Team.GetById(id.GetValueOrDefault());
            }
            return View(teamVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TeamVM teamVM)
        {
            // Remover la validación de la imagen si no se selecciona una nueva imagen
            ModelState.Remove("Team.ImageUrl");
            if (ModelState.IsValid)
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                var teamFromDb = _contenedorTrabajo.Team.GetById(teamVM.Team.Id);

                if (files.Count > 0)
                {
                    // Editar Imagen
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\teams");
                    var extension_new = Path.GetExtension(files[0].FileName);

                    // Eliminar la imagen anterior
                    if (!string.IsNullOrEmpty(teamFromDb.ImageUrl))
                    {
                        var imagePath = Path.Combine(webRootPath, teamFromDb.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    // Subir nueva imagen
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension_new), FileMode.Create))
                    {
                        files[0].CopyTo(fileStreams);
                    }

                    // Guardar la nueva ruta de la imagen
                    teamFromDb.ImageUrl = @"\images\teams\" + fileName + extension_new;
                }
                else
                {
                    // No se seleccionó una nueva imagen, mantener la imagen existente
                    teamVM.Team.ImageUrl = teamFromDb.ImageUrl;
                }

                _contenedorTrabajo.Team.Update(teamVM.Team);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }

            // Si el ModelState no es válido, volver a cargar las listas y retornar la vista
            teamVM.RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList();
            teamVM.CoachList = _contenedorTrabajo.Coach.GetCoachesList();
            return View(teamVM);
        }

        [HttpGet]
        public IActionResult Roster(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = _contenedorTrabajo.Team.GetById(id.Value);
            if (team == null)
            {
                return NotFound();
            }
            // Data del equipo
            team.Coach = _contenedorTrabajo.Coach.GetById(team.CoachId);
            // Obtener todos los usuarios que no están bloqueados
            var users = _contenedorTrabajo.User.GetAll(u => u.LockoutEnd == null && u.StudentId != null).Select(u => u.StudentId).ToList();
            // Lista de estudiantes que pertenecen al equipo y no están bloqueados como usuarios
            var students = _contenedorTrabajo.Student.GetAll(s => s.TeamId == id.Value && users.Contains(s.Id)).ToList();
            var captain = students.FirstOrDefault(s => s.IsCaptain);

            TeamVM teamVM = new()
            {
                Team = team,
                Captain = captain
            };

            return View(teamVM);
        }

        [HttpPost]
        public IActionResult ChangeCaptain(int teamId, int newCaptainId)
        {
            try
            {
                // Get current captain if exists
                var currentCaptain = _contenedorTrabajo.Student.GetAll(s => s.TeamId == teamId && s.IsCaptain).FirstOrDefault();

                // Remove captain status from current captain
                if (currentCaptain != null)
                {
                    currentCaptain.IsCaptain = false;
                    _contenedorTrabajo.Student.Update(currentCaptain);
                }  

                // Set new captain
                var newCaptain = _contenedorTrabajo.Student.GetById(newCaptainId);
                if (newCaptain == null || newCaptain.TeamId != teamId)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado o no pertenece al equipo" });
                }

                newCaptain.IsCaptain = true;
                _contenedorTrabajo.Student.Update(newCaptain);
                _contenedorTrabajo.Save();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cambiar el capitán: " + ex.Message });
            }
        }

        // Método para generar el PDF de un solo estudiante
        [HttpPost]
        public IActionResult GenerateStudentCertificate(int id)
        {
            // Obtener el estudiante por su ID
            var student = _contenedorTrabajo.Student.GetById(id);
            if (student == null)
            {
                return NotFound("Estudiante no encontrado");
            }

            // Crear el documento PDF usando QuestPDF con la información del estudiante
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);

                    // Aquí puedes personalizar la estructura del PDF con los datos del estudiante
                    page.Content().Element(c => CreateStudentCell(c, student));

                    page.Footer().Text(text => text.CurrentPageNumber());
                });
            });

            // Convertir el documento en un archivo PDF para la respuesta
            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Cedula_Estudiante_{student.Id}.pdf");
        }


        [HttpPost]
        private static void CreateStudentCell(IContainer container, Models.Student student)
        {
            string imageUrl = student.ImageUrl != null && student.ImageUrl.StartsWith("/") ? student.ImageUrl.Substring(1) : student.ImageUrl ?? "";
            imageUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('\\'));

            byte[] imageBytes = Array.Empty<byte>();

            if (System.IO.File.Exists(imageUrl))
            {
                imageBytes = System.IO.File.ReadAllBytes(imageUrl);
            }

            container.Padding(2)
                .Border(1)
                .BorderColor(Colors.Black)
                .Background(Colors.Grey.Lighten4)
                .DefaultTextStyle(x => x.FontSize(8).LineHeight(1.5f))
                .Column(column =>
                {
                    // Row for Image, Control Number, and Semester
                    column.Item().Padding(5).Row(row =>
                    {
                        row.ConstantItem(3f, Unit.Centimetre)
                            .Height(3f, Unit.Centimetre)
                            .Width(2.5f, Unit.Centimetre)
                            .AlignMiddle()
                            .AlignCenter()
                            .Image(imageBytes)
                            .FitArea();

                        row.RelativeItem().PaddingTop(20).Column(col =>
                        {
                            col.Item().Text("Número de control").Bold().AlignCenter();
                            col.Item().PaddingBottom(10).Text(student.ControlNumber ?? "Sin Actualizar").AlignCenter();

                            col.Item().Text("Semestre").Bold().AlignCenter();
                            col.Item().Text($"{(student.Semester != null ? $"{student.Semester}° semestre" : "Sin Actualizar")}").AlignCenter();
                        });
                    });

                    // Row for Personal Information
                    column.Item().PaddingLeft(10).PaddingBottom(5).Column(innerColumn =>
                    {
                        innerColumn.Item().Text("Nombre").Bold();
                        innerColumn.Item().Text($"{student.Name ?? "Sin Actualizar"} {student.LastName ?? "Sin Actualizar"} {student.SecondLastName ?? "Sin Actualizar"}");

                        innerColumn.Item().Text("Carrera").Bold();
                        innerColumn.Item().Text(student.Career ?? "Sin Actualizar");

                        innerColumn.Item().Text("CURP").Bold();
                        innerColumn.Item().Text(student.Curp ?? "Sin Actualizar");

                        innerColumn.Item().Text("Fecha de Nacimiento").Bold();
                        innerColumn.Item().Text(student.BirthDate ?? "Sin Actualizar");

                        innerColumn.Item().Text("Teléfono").Bold();
                        innerColumn.Item().Text(student.Phone ?? "Sin Actualizar");

                        innerColumn.Item().Text("Correo Electrónico").Bold();
                        innerColumn.Item().Text(student.Email ?? "Sin Actualizar");

                        innerColumn.Item().Text("Edad").Bold();
                        innerColumn.Item().Text(student.Age?.ToString() ?? "Sin Actualizar");

                        innerColumn.Item().Text("Tipo de Sangre").Bold();
                        innerColumn.Item().Text(student.BloodType ?? "Sin Actualizar");

                        innerColumn.Item().Text("Peso").Bold();
                        innerColumn.Item().Text(student.Weight?.ToString("0.0") ?? "Sin Actualizar");

                        innerColumn.Item().Text("Altura").Bold();
                        innerColumn.Item().Text(student.Height?.ToString("0.0") ?? "Sin Actualizar");

                        innerColumn.Item().Text("Alergias").Bold();
                        innerColumn.Item().Text(student.Allergies ?? "Sin Actualizar");

                        innerColumn.Item().Text("NSS").Bold();
                        innerColumn.Item().Text(student.Nss ?? "Sin Actualizar");

                        innerColumn.Item().Text("Número de Jugador").Bold();
                        innerColumn.Item().Text(student.numberUniform?.ToString() ?? "Sin Actualizar");

                        innerColumn.Item().Text("Fecha de Ingreso").Bold();
                        innerColumn.Item().Text(student.enrollmentData?.ToString() ?? "Sin Actualizar");

                        innerColumn.Item().Text("Entrenador").Bold();
                        innerColumn.Item().Text(student.Coach?.Name ?? "Sin Actualizar");

                        innerColumn.Item().Text("Equipo").Bold();
                        innerColumn.Item().Text(student.Team?.Name ?? "Sin Actualizar");

                        innerColumn.Item().Text("Capitán").Bold();
                        innerColumn.Item().Text(student.IsCaptain ? "Sí" : "No");
                    });

                    column.Item().PaddingTop(20).PaddingHorizontal(10).LineHorizontal(1).LineColor(Colors.Black);
                    column.Item().PaddingBottom(10).Text("Firma").AlignCenter();
                });
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Team.GetAll(includeProperties: "Representative,Coach") });
        }

        [HttpGet]
        public IActionResult GetStudentsByTeamId(int teamId)
        {
            var users = _contenedorTrabajo.User.GetAll(u => u.LockoutEnd == null && u.StudentId != null).Select(u => u.StudentId).ToList();
            // Lista de estudiantes que pertenecen al equipo y no están bloqueados como usuarios
            var students = _contenedorTrabajo.Student.GetAll(s => s.TeamId == teamId && users.Contains(s.Id)).ToList();
            return Json(new { data = students });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _contenedorTrabajo.Team.GetById(id);

            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error al borrar el equipo" });
            }

            // Verificar si el equipo tiene estudiantes asignados
            var teamWithStudents = _contenedorTrabajo.Student.GetAll(s => s.TeamId == id);

            //Si el equipo tiene alumnos asignados, no se puede borrar  
            if (teamWithStudents.Any())
            {
                return Json(new { success = false, message = "No se puede borrar el equipo porque tiene alumnos asignados" });
            }

            // Si llegamos aquí, podemos borrar la imagen y el equipo
            string webRootPath = _hostingEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, objFromDb.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _contenedorTrabajo.Team.Remove(objFromDb);
            _contenedorTrabajo.Save();
            return Json(new { success = true, message = "Equipo borrado exitosamente" });
        }

        #endregion

    }
}
