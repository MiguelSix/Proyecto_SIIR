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
        [HttpGet]
        public IActionResult GenerateCertificate()
        {
            // Crea el documento PDF
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);

                    page.Header().Text("Certificate of Registration").FontSize(20).Bold();
                    page.Content().Text("This document certifies the team's registration in the tournament.");
                    page.Footer().Text(text => text.CurrentPageNumber());
                });
            });

            // Renderiza el documento a un MemoryStream
            using var stream = new MemoryStream();
            document.GeneratePdf(stream);

            // Retorna el PDF como archivo
            stream.Position = 0; // Resetea la posición del stream al inicio
            return File(stream.ToArray(), "application/pdf", "Certificate.pdf");
        }
    }
}
