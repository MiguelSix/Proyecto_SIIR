using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;
using System.IO;
using System.Security.Claims;

namespace SIIR.Areas.Coach.Controllers
{
    [Area("Coach")]
    [Authorize(Roles = "Coach")]
    public class TeamController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;

        public TeamController(IContenedorTrabajo contenedorTrabajo)
        {
            _contenedorTrabajo = contenedorTrabajo;
        }

        // Método para mostrar la vista de generación de certificado
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

        // Método para generar el PDF
        [HttpPost]
        public IActionResult GenerateCertificate([FromBody] CertificateRequest request)
        {
            if (request.Students == null || request.Students.Count == 0)
            {
                return BadRequest("No se han seleccionado estudiantes.");
            }

            // Crear el documento PDF usando QuestPDF y agregar la información de los estudiantes
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);


                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Añadir filas para los estudiantes en un bucle
                        for (int i = 0; i < request.Students.Count; i++)
                        {
                            table.Cell().Element(c => CreateStudentCellCertificate(c, request.Students[i]));
                        }
                        if (request.Coach != null)
                            table.Cell().Element(c => CreateCoachCellCertificate(c, request.Coach, request.Team));
                    });

                    page.Footer().Text(text => text.CurrentPageNumber());
                });
            });

            // Generar y devolver el PDF
            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/pdf", "Cedula.pdf");
        }

        private static void CreateStudentCellCertificate(IContainer container, Models.Student student)
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
                .DefaultTextStyle(x => x.FontSize(8).LineHeight(1.5f))
                .Column(column =>
                {

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
                            col.Item().PaddingBottom(10).Text(student.ControlNumber ?? "Sin actualizar").AlignCenter();

                            col.Item().Text("Semestre").Bold().AlignCenter();
                            col.Item().Text($"{(student.Semester != null ? $"{student.Semester}° semestre" : "Sin actualizar")}").AlignCenter();
                        });
                    });

                    column.Item().PaddingLeft(10).PaddingBottom(5).Column(innerColumn =>
                    {
                        innerColumn.Item().Text("Nombre").Bold();
                        innerColumn.Item().Text($"{student.Name ?? "Sin actualizar"} {student.LastName ?? "Sin actualizar"} {student.SecondLastName ?? "Sin actualizar"}");

                        innerColumn.Item().Text("Carrera").Bold();
                        innerColumn.Item().Text($"{student.Career ?? "Sin actualizar"}");

                        innerColumn.Item().Text("Nivel Académico").Bold();
                        innerColumn.Item().Text("Licenciatura");

                        innerColumn.Item().Text("Fecha de ingreso").Bold();
                        innerColumn.Item().Text($"{(student.enrollmentData.HasValue ? student.enrollmentData : "Sin actualizar")}");

                    });

                    column.Item().PaddingTop(20).PaddingHorizontal(10).LineHorizontal(1).LineColor(Colors.Black);
                    column.Item().PaddingBottom(10).Text("Firma").AlignCenter();
                });
        }

        private static void CreateCoachCellCertificate(IContainer container, Models.Coach coach, string teamName)
        {
            string imageUrl = coach.ImageUrl != null && coach.ImageUrl.StartsWith("/")
                ? coach.ImageUrl.Substring(1)
                : coach.ImageUrl ?? "";

            imageUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('\\'));

            if (!System.IO.File.Exists(imageUrl))
            {
                imageUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "zorro_default.png");
            }

            byte[] imageBytes = Array.Empty<byte>();

            imageBytes = System.IO.File.ReadAllBytes(imageUrl);

            container
                .Padding(2)
                .Border(1)
                .BorderColor(Colors.Black)
                .Background(Colors.Grey.Lighten4) // Fondo gris claro
                .Column(column =>
                {
                    // Cabecera: nombre del equipo centrado
                    column.Item().PaddingVertical(15).Text(teamName).FontSize(10).Bold().LineHeight(1.5f).AlignCenter();
                    column.Item().Text("Entrenador").FontSize(8).AlignCenter();

                    // Imagen del entrenador centrada
                    column.Item().AlignCenter().Element(container =>
                    {
                        container
                            .Width(4f, Unit.Centimetre)
                            .Height(5f, Unit.Centimetre)
                            .Image(imageBytes)
                            .FitArea();
                    });


                    // Nombre del entrenador debajo de la imagen, centrado
                    column.Item().PaddingTop(10).PaddingBottom(20).Text($"{coach.Name} {coach.LastName} {coach.SecondLastName}").FontSize(12).AlignCenter();
                });
        }

        #region API CALLS

        [HttpGet]
		public IActionResult GetStudentsByTeamId(int teamId)
		{
			var users = _contenedorTrabajo.User.GetAll(u => u.LockoutEnd == null && u.StudentId != null).Select(u => u.StudentId).ToList();
			// Lista de estudiantes que pertenecen al equipo y no están bloqueados como usuarios
			var students = _contenedorTrabajo.Student.GetAll(s => s.TeamId == teamId && users.Contains(s.Id));
			return Json(new { data = students });
		}


        #endregion
    }
}