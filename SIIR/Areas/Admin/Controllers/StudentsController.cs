using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StudentsController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public StudentsController(IContenedorTrabajo contenedorTrabajo,
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
        public IActionResult Edit(int id)
        {
            var student = _contenedorTrabajo.Student.GetFirstOrDefault(
                s => s.Id == id,
                includeProperties: "Team,Coach"
            );

            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.Student student)
        {
            // Quitar Coach y Team del ModelState
            ModelState.Remove("Coach");
            ModelState.Remove("Team");

            if (ModelState.IsValid)
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                var studentFromDb = _contenedorTrabajo.Student.GetById(student.Id);

                if (studentFromDb == null)
                {
                    return NotFound();
                }

                if (files.Count > 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\students");
                    var extension = Path.GetExtension(files[0].FileName);

                    if (!string.IsNullOrEmpty(studentFromDb.ImageUrl))
                    {
                        var imagePath = Path.Combine(webRootPath, studentFromDb.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    student.ImageUrl = @"\images\students\" + fileName + extension;
                }
                else
                {
                    student.ImageUrl = studentFromDb.ImageUrl;
                }

                _contenedorTrabajo.Student.Update(student);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
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
                            rightColumn.Item().Text(student.Weight?.ToString("0.0") ?? "Sin Actualizar");

                            rightColumn.Item().Text("Altura").Bold();
                            rightColumn.Item().Text(student.Height?.ToString("0.0") ?? "Sin Actualizar");

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


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Student.GetAll(includeProperties: "Team,Coach") });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _contenedorTrabajo.Student.GetById(id);
            var user = _contenedorTrabajo.User.GetAll(u => u.StudentId == id).FirstOrDefault();

            string webRootPath = _hostingEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, objFromDb.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting." });
            }

            _contenedorTrabajo.User.Remove(user);
            _contenedorTrabajo.Student.Remove(objFromDb);
            _contenedorTrabajo.Save();
            return Json(new { success = true, message = "Delete successful." });
        }

        #endregion
    }
}
