using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;
using System.IO.Compression;
using System.Linq;
using static QuestPDF.Helpers.Colors;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DocumentController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment; // Entorno de hospedaje para obtener rutas de archivos
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentController(
            IContenedorTrabajo contenedorTrabajo,
            IWebHostEnvironment hostingEnvironment,
            UserManager<ApplicationUser> userManager
            )
        {
            _contenedorTrabajo = contenedorTrabajo;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Coach, Student")]
        public async Task<IActionResult> Index(int studentId)
        {
            var documents = _contenedorTrabajo.Document.GetDocumentsByStudent(studentId);
            var student = _contenedorTrabajo.Student.GetFirstOrDefault(s => s.Id == studentId);

            if (student == null)
            {
                TempData["Error"] = "Estudiante no encontrado.";
                return RedirectToAction("Index", "Home");
            }

            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // If the user is a student (captain)
            if (user.StudentId != null)
            {
                // Get the captain's data
                var captain = _contenedorTrabajo.Student.GetFirstOrDefault(s => s.Id == user.StudentId);
                if (captain == null)
                {
                    return NotFound();
                }

                // Check if the student is captain
                if (!captain.IsCaptain)
                {
                    return Forbid();
                }

                // Verify if the requested student is from the same team as the captain
                if (student.TeamId != captain.TeamId)
                {
                    return Forbid();
                }
            }

            // Create the view model with the selected student's documents and information
            var documentVM = new DocumentVM
            {
                StudentDocuments = documents,
                Student = student
            };

            return View(documentVM);
        }

        // Acción para mostrar los detalles de un documento
        [HttpGet]
        [Authorize(Roles = "Admin, Coach, Student")]
        public async Task<IActionResult> Details(int id)
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            var stu = new Models.Student();
            if (user == null)
            {
                return NotFound();
            }

            // the user is a student
            if (user.StudentId != null)
            {
                // Get the student
                stu = _contenedorTrabajo.Student.GetFirstOrDefault(s => s.Id == user.StudentId);
                if (stu == null)
                {
                    return NotFound();
                }

                // Check if the student is captain
                if (!stu.IsCaptain)
                {
                    return Forbid();
                }
            }

            // Obtener el documento con su catálogo relacionado
            var document = _contenedorTrabajo.Document.GetDocumentWithCatalog(id);
            if (document == null)
            {
                return NotFound();
            }
            
            var student = _contenedorTrabajo.Student.GetFirstOrDefault(s => s.Id == document.StudentId);
            if (student == null)
            {
                return NotFound();
            }

            var documentVM = new DocumentVM
            {
                Document = document,
                Student = student
            };

            // Verifica que el archivo existe
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, document.Url.TrimStart('\\'));
            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "El archivo no se encuentra en el servidor.";
            }

            return View(documentVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult ChangeStatus(int id, string status, string rejectionReason)
        {
            var document = _contenedorTrabajo.Document.GetDocumentWithCatalog(id);
            if (document == null)
            {
                return Json(new { success = false, message = "Documento no encontrado." });
            }

            // Cambiar el estado del documento al nuevo estado proporcionado
            document.Status = (DocumentStatus)Enum.Parse(typeof(DocumentStatus), status);

            //Si el estado es rechazado, guardamos el motivo
            if(document.Status == DocumentStatus.Rejected)
            {
                // Validar que haya un motivo de rechazo cuando el estado es Rejected
                if (string.IsNullOrEmpty(rejectionReason))
                {
                    return Json(new { success = false, message = "El motivo de rechazo es requerido." });
                }
                document.RejectionReason = rejectionReason;

                var notification = new Notification
                {
                    StudentId = document.Student.Id,
                    Message = $"Tu documento '{document.DocumentCatalog.Name}' ha sido rechazado. Razón: {rejectionReason}",
                    Type = "DocumentRejected",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    DocumentId = document.Id
                };

                _contenedorTrabajo.Notification.Add(notification);
            }
            else 
            {
                document.RejectionReason = string.Empty;
            }

            _contenedorTrabajo.Document.Update(document);
            _contenedorTrabajo.Save();

            return RedirectToAction(nameof(Index), new { studentId = document.StudentId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Coach, Student")]
        public async Task<IActionResult> Download(int id)
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            var stu = new Models.Student();
            if (user == null)
            {
                return NotFound();
            }

            // the user is a student
            if (user.StudentId != null)
            {
                // Get the student
                stu = _contenedorTrabajo.Student.GetFirstOrDefault(s => s.Id == user.StudentId);
                if (stu == null)
                {
                    return NotFound();
                }

                // Check if the student is captain
                if (!stu.IsCaptain)
                {
                    return Forbid();
                }
            }

            var document = _contenedorTrabajo.Document.GetFirstOrDefault(d => d.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, document.Url.TrimStart('\\'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileName = Path.GetFileName(filePath);

            return PhysicalFile(filePath, "application/octet-stream", fileName);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Coach, Student")]
        public async Task<IActionResult> DownloadAll(int studentId)
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            var stu = new Models.Student();
            if (user == null)
            {
                return NotFound();
            }

            // the user is a student
            if (user.StudentId != null)
            {
                // Get the student
                stu = _contenedorTrabajo.Student.GetFirstOrDefault(s => s.Id == user.StudentId);
                if (stu == null)
                {
                    return NotFound();
                }

                // Check if the student is captain
                if (!stu.IsCaptain)
                {
                    return Forbid();
                }
            }

            var documents = _contenedorTrabajo.Document.GetDocumentsByStudent(studentId);
            var student = _contenedorTrabajo.Student.GetFirstOrDefault(s => s.Id == studentId);

            if (!documents.Any() || student == null)
            {
                TempData["Error"] = "No hay documentos para descargar o el estudiante no existe.";
                return RedirectToAction(nameof(Index), new { studentId });
            }

            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            try
            {
                // Crear archivo ZIP
                var zipPath = Path.Combine(tempPath, $"Documentos_{student.ControlNumber}.zip");
                using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    foreach (var doc in documents)
                    {
                        var filePath = Path.Combine(_hostingEnvironment.WebRootPath, doc.Url.TrimStart('\\'));
                        if (System.IO.File.Exists(filePath))
                        {
                            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                            var extension = Path.GetExtension(filePath);
                            var fileName = $"{doc.DocumentCatalog.Name}_{student.ControlNumber}_{timestamp}{extension}";

                            archive.CreateEntryFromFile(filePath, fileName);
                        }
                    }
                }

                // Leer el archivo ZIP y devolverlo como FileResult
                var zipBytes = System.IO.File.ReadAllBytes(zipPath);

                // Limpiar archivos temporales
                Directory.Delete(tempPath, true);

                return File(zipBytes, "application/zip", $"Documentos_{student.ControlNumber}.zip");
            }
            catch (Exception ex)
            {
                // Asegurarse de limpiar en caso de error
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }

                TempData["Error"] = "Error al generar el archivo ZIP: " + ex.Message;
                return RedirectToAction(nameof(Index), new { studentId });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Coach, Student")]
        public async Task<IActionResult> DownloadAllByTeam(int teamId)
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            var stu = new Models.Student();
            if (user == null)
            {
                return NotFound();
            }

            // the user is a student
            if (user.StudentId != null)
            {
                // Get the student
                stu = _contenedorTrabajo.Student.GetFirstOrDefault(s => s.Id == user.StudentId);
                if (stu == null)
                {
                    return NotFound();
                }

                // Check if the student is captain
                if (!stu.IsCaptain)
                {
                    return Forbid();
                }
            }

            var students = _contenedorTrabajo.Student.GetAll(s => s.TeamId == teamId);
            var team = _contenedorTrabajo.Team.GetById(teamId);

            if (!students.Any() || team == null)
            {
                TempData["Error"] = "No hay estudiantes en el equipo o el equipo no existe.";
                return RedirectToAction(nameof(Index), new { studentId = teamId });
            }

            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            try
            {
                // Crear archivo ZIP principal
                var teamZipPath = Path.Combine(tempPath, $"{team.Name}.zip");
                using (var teamZipArchive = ZipFile.Open(teamZipPath, ZipArchiveMode.Create))
                {
                    foreach (var student in students)
                    {
                        var studentDocuments = _contenedorTrabajo.Document.GetDocumentsByStudent(student.Id);
                        if (studentDocuments.Any())
                        {
                            // Crear archivo ZIP individual para el estudiante
                            var studentZipPath = Path.Combine(tempPath, $"{student.Name}_{student.LastName}_{student.ControlNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
                            using (var studentZipArchive = ZipFile.Open(studentZipPath, ZipArchiveMode.Create))
                            {
                                foreach (var doc in studentDocuments)
                                {
                                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, doc.Url.TrimStart('\\'));
                                    if (System.IO.File.Exists(filePath))
                                    {
                                        var fileName = $"{doc.DocumentCatalog.Name}_{student.ControlNumber}{Path.GetExtension(filePath)}";
                                        studentZipArchive.CreateEntryFromFile(filePath, fileName);
                                    }
                                }
                            }

                            // Agregar el archivo ZIP individual al archivo ZIP principal
                            teamZipArchive.CreateEntryFromFile(studentZipPath, Path.GetFileName(studentZipPath));
                        }
                    }
                }
                // Leer el archivo ZIP principal y devolverlo como FileResult
                var teamZipBytes = System.IO.File.ReadAllBytes(teamZipPath);

                // Limpiar archivos temporales
                Directory.Delete(tempPath, true);

                return File(teamZipBytes, "application/zip", $"{team.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
            }
            catch (Exception ex)
            {
                // Asegurarse de limpiar en caso de error
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }

                TempData["Error"] = "Error al generar el archivo ZIP: " + ex.Message;
                return RedirectToAction(nameof(Index), new { studentId = teamId });
            }
        }

    }
}