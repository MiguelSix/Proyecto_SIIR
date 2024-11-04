using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
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
        public IActionResult Index(int id)
        {
            Team team = _contenedorTrabajo.Team.GetById(id);
            team.Coach = _contenedorTrabajo.Coach.GetById(team.CoachId);
            return View(team);
        }

        [HttpGet]
        public IActionResult GetAllStudents(int teamId)
        {
            var students = _contenedorTrabajo.Student.GetAll(t =>
                t.TeamId == teamId);

            return Json(new { data = students });
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
                        for (int i = 0; i < request.Students.Count; i ++)
                        {
                            table.Cell().Element(c => CreateStudentCell(c, request.Students[i]));
                        }

                        table.Cell().Element(c => CreateCoachCell(c, request.Coach, request.Team));
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

        private static void CreateStudentCell(IContainer container, Models.Student student)
        {
            string imageUrl = student.ImageUrl.StartsWith("/") ? student.ImageUrl.Substring(1) : student.ImageUrl;
            imageUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('\\'));

            byte[] imageBytes;
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
                        col.Item().PaddingBottom(10).Text(student.ControlNumber ?? "---").AlignCenter();
                        
                        col.Item().Text("Semestre").Bold().AlignCenter();
                        col.Item().Text($"{(student.Semester != null ? $"{student.Semester}° semestre" : "---")}").AlignCenter();
                    });
                });

                column.Item().PaddingLeft(10).PaddingBottom(5).Column(innerColumn =>
                {
                    innerColumn.Item().Text("Nombre").Bold();
                    innerColumn.Item().Text($"{student.Name ?? "---"} {student.LastName ?? "---"} {student.SecondLastName ?? "---"}");

                    innerColumn.Item().Text("Carrera").Bold();
                    innerColumn.Item().Text($"{student.Career ?? "---"}");

                    innerColumn.Item().Text("Nivel Académico").Bold();
                    innerColumn.Item().Text("Licenciatura");

                    innerColumn.Item().Text("Fecha de ingreso").Bold();
                    innerColumn.Item().Text("2002");
                });

                column.Item().PaddingTop(20).PaddingHorizontal(10).LineHorizontal(1).LineColor(Colors.Black);     
                column.Item().PaddingBottom(10).Text("Firma").AlignCenter();
            });
        }

        private static void CreateCoachCell(IContainer container, Models.Coach coach, string teamName)
        {
            string imageUrl = coach.ImageUrl.StartsWith("/") ? coach.ImageUrl.Substring(1) : coach.ImageUrl;
            imageUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('\\'));

            byte[] imageBytes;
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
                        column.Item().PaddingTop(10).PaddingBottom(20).Text($"{ coach.Name } { coach.LastName } { coach.SecondLastName }").FontSize(12).AlignCenter();
                });
        }
    }
}

public class CertificateRequest
{
    public List<Student>? Students { get; set; }
    public Coach? Coach { get; set; }
    public string? Team { get; set; }
}