using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIIR.Data;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models; // Asegúrate de incluir tu modelo aquí
using SIIR.Models.ViewModels;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SIIR.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class DocumentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private const int MaxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly ILogger<DocumentController> _logger;
        public DocumentController(UserManager<ApplicationUser> userManager, IContenedorTrabajo contenedorTrabajo, IWebHostEnvironment hostingEnvironment, ILogger<DocumentController> logger)
        {
            _userManager = userManager;
            _contenedorTrabajo = contenedorTrabajo;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            //var user = await _userManager.GetUserAsync(User);
            //if (user == null)
            //{
            //    return NotFound();
            //}

            //var student = await _userManager.Users
            //    .Where(u => u.Id == user.Id)
            //    .Select(u => u.Student)
            //    .FirstOrDefaultAsync();

            //if (student == null)
            //{
            //    return NotFound();
            //}

            //var studentId = student.Id; // Suponiendo que 'Id' es el identificador del estudiante

            var student = await GetCurrentStudent();

            if (student == null)
            {
                return NotFound();
            }

            var studentId = student.Id; // Suponiendo que 'Id' es el identificador del estudiante

            // Obtener la lista de tipos de documentos del catálogo
            var listaDocumentos = _contenedorTrabajo.DocumentCatalog.GetAll().Select(doc => new SelectListItem
            {
                Text = doc.Name,
                Value = doc.Id.ToString(),
                Group = new SelectListGroup { Name = doc.Description },
            });

            // Obtener los documentos existentes del estudiante
            var documentosExistentes = _contenedorTrabajo.Document.GetDocumentsByStudent(studentId);

            // Crear el ViewModel con toda la información
            DocumentVM docuVM = new DocumentVM()
            {
                Document = new Document(),
                ListDocumenCatalog = listaDocumentos,
                StudentDocuments = documentosExistentes
            };

            return View(docuVM);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> SaveDocument(IFormFile file, int documentCatalogId)
        {
            try
            {
                // Validaciones iniciales permanecen igual...
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "No se ha seleccionado ningún archivo." });
                }

                // Obtener el tipo de documento del catálogo
                var documentCatalog = _contenedorTrabajo.DocumentCatalog.GetById(documentCatalogId);
                if (documentCatalog == null)
                {
                    return Json(new { success = false, message = "Tipo de documento no válido." });
                }

                // Validaciones de tamaño y extensión permanecen igual...
                if (file.Length > MaxFileSize)
                {
                    return Json(new { success = false, message = "El archivo no debe exceder los 5MB." });
                }

                

                // Validar extensión según el tipo de documento
                string extension = Path.GetExtension(file.FileName).ToLower();
                bool isValidExtension = false;

                if (documentCatalog.Extension == "pdf")
                {
                    isValidExtension = extension == ".pdf";
                    if (!isValidExtension)
                    {
                        return Json(new { success = false, message = "Solo se permiten archivos PDF." });
                    }
                }
                else if (documentCatalog.Extension == "image")
                {
                    string[] allowedImageExtensions = { ".jpg", ".jpeg", ".png" };
                    isValidExtension = allowedImageExtensions.Contains(extension);
                    if (!isValidExtension)
                    {
                        return Json(new { success = false, message = "Solo se permiten archivos JPG, JPEG o PNG." });
                    }
                }

                // Obtener estudiante
                var student = await GetCurrentStudent();
                if (student == null)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado." });
                }

                // Verificar documento existente
                var existingDocument = _contenedorTrabajo.Document
                    .GetAll()
                    .FirstOrDefault(d => d.StudentId == student.Id &&
                                       d.DocumentCatalogId == documentCatalogId);

                string rutaPrincipal = _hostingEnvironment.WebRootPath;

                // Generar el nuevo nombre del archivo
                string fechaHora = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                // Limpiar el nombre del documento para evitar caracteres especiales
                string documentoNombreLimpio = documentCatalog.Name.Replace(" ", "_")
                                                             .Replace(",", "")
                                                             .Replace(".", "")
                                                             .Replace("/", "")
                                                             .Replace("\\", "");
                string nombreArchivo = $"{documentoNombreLimpio}_student{student.Id}_{fechaHora}";

                var subidas = Path.Combine(rutaPrincipal, @"student\documents");

                // Asegurar que el directorio existe
                if (!Directory.Exists(subidas))
                {
                    Directory.CreateDirectory(subidas);
                }

                var filePath = Path.Combine(subidas, nombreArchivo + extension);

                // Si existe un documento previo, eliminar el archivo físico
                if (existingDocument != null)
                {
                    var oldFilePath = Path.Combine(rutaPrincipal, existingDocument.Url.TrimStart('\\'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Guardar el nuevo archivo
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                if (existingDocument != null)
                {
                    // Actualizar documento existente
                    existingDocument.Url = @"\student\documents\" + nombreArchivo + extension;
                    existingDocument.UploadDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    existingDocument.Status = DocumentStatus.Pending;
                    _contenedorTrabajo.Document.Update(existingDocument);
                }
                else
                {
                    // Crear nuevo documento
                    var newDocument = new Document
                    {
                        Url = @"\student\documents\" + nombreArchivo + extension,
                        UploadDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                        Status = DocumentStatus.Pending,
                        StudentId = student.Id,
                        DocumentCatalogId = documentCatalogId
                    };
                    _contenedorTrabajo.Document.Add(newDocument);
                }

                // Guardar cambios en la base de datos
                _contenedorTrabajo.Save();

                return Json(new
                {
                    success = true,
                    message = "Documento guardado exitosamente."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar documento");
                return Json(new { success = false, message = $"Error al guardar el documento: {ex.Message}" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> DownloadDocument(int documentCatalogId)
        {
            try
            {
                var student = await GetCurrentStudent();
                if (student == null)
                {
                    _logger.LogWarning("Intento de descarga por usuario no autorizado");
                    return Unauthorized();
                }

                var document = _contenedorTrabajo.Document
                    .GetFirstOrDefault(d => d.StudentId == student.Id &&
                                          d.DocumentCatalogId == documentCatalogId,
                                          includeProperties: "DocumentCatalog");

                if (document == null)
                {
                    _logger.LogWarning($"Documento no encontrado. DocumentCatalogId: {documentCatalogId}, StudentId: {student.Id}");
                    return NotFound("Documento no encontrado.");
                }

                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, document.Url.TrimStart('\\', '/'));

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogError($"Archivo físico no encontrado en: {filePath}");
                    return NotFound("Archivo físico no encontrado.");
                }

                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                var mimeType = extension switch
                {
                    ".pdf" => "application/pdf",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    _ => "application/octet-stream"
                };

                var timestamp = DateTime.Now;
                var downloadFileName = $"{document.DocumentCatalog.Name}_{student.Id}_{timestamp:yyyyMMdd_HHmmss}{extension}";

                // Usar FileStreamResult con ContentDisposition explícito
                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return new FileStreamResult(stream, mimeType)
                {
                    FileDownloadName = downloadFileName,
                    EnableRangeProcessing = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al descargar documento {documentCatalogId}");
                return BadRequest("Error al procesar la descarga del documento.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocument(int documentCatalogId)
        {
            try
            {
                // Obtener el estudiante actual
                var student = await GetCurrentStudent();
                if (student == null)
                {
                    return Json(new { success = false, message = "Usuario no autorizado." });
                }

                // Buscar el documento
                var document = _contenedorTrabajo.Document
                    .GetFirstOrDefault(d => d.StudentId == student.Id &&
                                          d.DocumentCatalogId == documentCatalogId);

                if (document == null)
                {
                    return Json(new { success = false, message = "Documento no encontrado." });
                }

                // Obtener la ruta del archivo
                string rutaPrincipal = _hostingEnvironment.WebRootPath;
                var filePath = Path.Combine(rutaPrincipal, document.Url.TrimStart('\\', '/'));

                // Eliminar el archivo físico si existe
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, $"Error al eliminar el archivo físico: {filePath}");
                        return Json(new { success = false, message = "Error al eliminar el archivo físico." });
                    }
                }

                // Eliminar el registro de la base de datos
                _contenedorTrabajo.Document.Remove(document);
                _contenedorTrabajo.Save();

                return Json(new { success = true, message = "Documento eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar documento {documentCatalogId}");
                return Json(new { success = false, message = "Error al procesar la eliminación del documento." });
            }
        }

        // Método helper para obtener el estudiante actual
        private async Task<SIIR.Models.Student> GetCurrentStudent()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return null;
            }

            return await _userManager.Users
                .Where(u => u.Id == user.Id)
                .Select(u => u.Student)
                .FirstOrDefaultAsync();
        }



        #region Llamadas a la API
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Document.GetAll(includeProperties: "DocumentCatalog") });
        }
       
        #endregion
    }
}
