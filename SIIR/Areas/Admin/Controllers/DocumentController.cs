using Microsoft.AspNetCore.Mvc;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;
using System.IO.Compression;
using System.Linq;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DocumentController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment; // Entorno de hospedaje para obtener rutas de archivos

        public DocumentController(IContenedorTrabajo contenedorTrabajo, IWebHostEnvironment hostingEnvironment)
        {
            _contenedorTrabajo = contenedorTrabajo;
            _hostingEnvironment = hostingEnvironment;
        }


        public IActionResult Index(int studentId)
        {
            var documents = _contenedorTrabajo.Document.GetDocumentsByStudent(studentId);
            var student = _contenedorTrabajo.Student.GetFirstOrDefault(s => s.Id == studentId);
            
            if (student == null)
            {
                TempData["Error"] = "Estudiante no encontrado."; // Mensaje de error si no se encuentra el estudiante
                return RedirectToAction("Index", "Home");
            }

            // Crear una vista modelo con los documentos del estudiante
            var documentVM = new DocumentVM
            {
                StudentDocuments = documents,
                Student = student
            };

            return View(documentVM);
        }
        
        // Acción para mostrar los detalles de un documento
        [HttpGet]
        public IActionResult Details(int id)
        {

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
        public IActionResult ChangeStatus(int id, string status)
        {
            var document = _contenedorTrabajo.Document.GetFirstOrDefault(d => d.Id == id);
            if (document == null)
            {
                return Json(new { success = false, message = "Documento no encontrado." });
            }

            // Cambiar el estado del documento al nuevo estado proporcionado
            document.Status = (DocumentStatus)Enum.Parse(typeof(DocumentStatus), status);
            _contenedorTrabajo.Document.Update(document);
            _contenedorTrabajo.Save();

            return RedirectToAction(nameof(Index), new { studentId = document.StudentId });
        }

        [HttpGet]
        public IActionResult Download(int id)
        {
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
        public IActionResult DownloadAll(int studentId)
        {
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



    }
}