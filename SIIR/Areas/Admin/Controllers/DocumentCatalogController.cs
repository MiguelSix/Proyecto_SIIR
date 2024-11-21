using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Utilities;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DocumentCatalogController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public DocumentCatalogController(
            IContenedorTrabajo contenedorTrabajo,
            IEmailSender emailSender,
            IWebHostEnvironment hostingEnvironment
            )
        {
            _contenedorTrabajo = contenedorTrabajo;
            _emailSender = emailSender;
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
            LoadExtensions();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DocumentCatalog documentCatalog)
        {
            // Verificar si el nombre del documento ya existe en la base de datos
            var existDocument = _contenedorTrabajo.DocumentCatalog.GetAll()
                                    .Any(d => d.Name.ToLower() == documentCatalog.Name.ToLower());

            if (existDocument)
            {
                // Si existe un documento con el mismo nombre, agregar un error al ModelState
                ModelState.AddModelError("Nombre", "Ya existe un documento con este nombre.");
            }


            if (ModelState.IsValid)
            {
                _contenedorTrabajo.DocumentCatalog.Add(documentCatalog);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }

            // Si no es válido, cargar nuevamente las extensiones
            LoadExtensions();

            return View(documentCatalog);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            LoadExtensions();

            DocumentCatalog documentCatalog = _contenedorTrabajo.DocumentCatalog.GetById(id);
            if (documentCatalog == null)
            {
                return NotFound();
            }
            return View(documentCatalog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DocumentCatalog documentCatalog)
        {
            // Verificar si ya existe otro documento con el mismo nombre, excluyendo el documento actual
            var existDocument = _contenedorTrabajo.DocumentCatalog.GetAll()
                                    .Any(d => d.Name.ToLower() == documentCatalog.Name.ToLower()
                                           && d.Id != documentCatalog.Id); // Excluir el documento actual por ID
            if (existDocument)
            {
                // Si existe un documento con el mismo nombre, agregar un error al ModelState
                ModelState.AddModelError("Nombre", "Ya existe un documento con este nombre.");
            }

            if (ModelState.IsValid)
            {
                _contenedorTrabajo.DocumentCatalog.Update(documentCatalog);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }

            // Si no es válido, cargar nuevamente las extensiones
            LoadExtensions();

            return View(documentCatalog);
        }
        private void LoadExtensions()
        {
            var extensiones = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Selecciona el tipo de documento", Selected = true, Disabled = true }, // Opción predeterminada
                new SelectListItem { Value = "pdf", Text = "PDF" },
                new SelectListItem { Value = "foto", Text = "Foto" }
            };

            // Pasar las extensiones a la vista a través de ViewBag
            ViewBag.Extensiones = extensiones;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RefrendoDocumentos([FromBody] List<int> documentIds)
        {
            try
            {
                

                if (documentIds == null || !documentIds.Any())
                {
                    return Json(new { success = false, message = "No se han seleccionado documentos" });
                }

                var estudiantes = _contenedorTrabajo.Student.GetAll();

                foreach (var estudiante in estudiantes)
                {
                    foreach (var documentId in documentIds)
                    {
                        // Obtener el documento del catálogo
                        var documentoCatalogo = _contenedorTrabajo.DocumentCatalog.GetById(documentId);
                        if (documentoCatalogo == null) continue;

                        // Buscar si existe un documento actual
                        var documentoExistente = _contenedorTrabajo.Document
                            .GetFirstOrDefault(d => d.StudentId == estudiante.Id &&
                                                  d.DocumentCatalogId == documentId);

                        if (documentoExistente != null)
                        {
                            documentoExistente.Status = DocumentStatus.RequiresRenewal;
                        

                            // Crear notificación para el estudiante
                            var notification = new Notification
                            {
                                StudentId = estudiante.Id,
                                Message = $"El documento '{documentoCatalogo.Name}' debe actualizarse/refrendarse, favor de subir nuevamente el documento",
                                Type = "DocumentRefrendo",
                                IsRead = false,
                                CreatedAt = DateTime.Now.AddHours(-6),
                                DocumentId = documentoExistente.Id
                            };

                            _contenedorTrabajo.Notification.Add(notification);

                            // Enviar correo electrónico al estudiante
                            var student = estudiante;
                            var user = _contenedorTrabajo.User.GetFirstOrDefault(u => u.StudentId == student.Id);
                            if (user != null)
                            {
                                var emailBody = string.Format(
                                    await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.WebRootPath, "templates", "document-renewal.html")),
                                    estudiante.Name, // {0}
                                    documentoCatalogo.Name // {1}
                                );

                                await _emailSender.SendEmailAsync(
                                    user.Email,
                                    "Actualización de Documento Requerida",
                                    emailBody
                                );
                            }
                        }
                    }
                }
                _contenedorTrabajo.Save();

                return Json(new { success = true, message = "Documentos marcados para refrendo exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al procesar el refrendo: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendReminders()
        {
            try
            {
                var estudiantes = _contenedorTrabajo.Student.GetAll();
                var documentosCatalogo = _contenedorTrabajo.DocumentCatalog.GetAll().ToList();
                int recordatoriosEnviados = 0;

                // Crear una lista para almacenar todas las notificaciones
                var notificacionesParaAgregar = new List<Notification>();

                foreach (var estudiante in estudiantes)
                {
                    // Obtener documentos del estudiante
                    var documentosEstudiante = _contenedorTrabajo.Document
                        .GetAll(d => d.StudentId == estudiante.Id)
                        .Select(d => d.DocumentCatalogId)
                        .ToList();

                    // Encontrar documentos faltantes
                    var documentosFaltantes = documentosCatalogo
                        .Where(dc => !documentosEstudiante.Contains(dc.Id))
                        .ToList();

                    if (documentosFaltantes.Any())
                    {
                        var user = _contenedorTrabajo.User.GetFirstOrDefault(u => u.StudentId == estudiante.Id);
                        if (user != null)
                        {
                            try
                            {
                                // Crear el listado de documentos para el correo
                                var listaDocumentos = string.Join("\n",
                                    documentosFaltantes.Select(d => $"<li style=\"margin-bottom: 5px;\">{d.Name}</li>"));

                                var emailBody = string.Format(
                                    await System.IO.File.ReadAllTextAsync(
                                        Path.Combine(_hostingEnvironment.WebRootPath, "templates", "document-reminder.html")),
                                    estudiante.Name,
                                    listaDocumentos
                                );

                                // Enviar el correo
                                await _emailSender.SendEmailAsync(
                                    user.Email,
                                    "Recordatorio: Documentos Pendientes",
                                    emailBody
                                );

                                // Crear una notificación por cada documento faltante
                                var notification = new Notification
                                {
                                    StudentId = estudiante.Id,
                                    DocumentId = null,
                                    Message = $"Tienes {documentosFaltantes.Count} documento(s) pendiente(s) por subir. Por favor, revisa tu correo electrónico.",
                                    Type = "DocumentReminder",
                                    IsRead = false,
                                    CreatedAt = DateTime.UtcNow.AddHours(-6)
                                };

                                notificacionesParaAgregar.Add(notification);
                                recordatoriosEnviados++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error al procesar estudiante {estudiante.Id}: {ex.Message}");
                                continue;
                            }
                        }
                    }
                }

                // Agregar todas las notificaciones
                foreach (var notificacion in notificacionesParaAgregar)
                {
                    _contenedorTrabajo.Notification.Add(notificacion);
                }

                // Guardar todos los cambios
                _contenedorTrabajo.Save();

                return Json(new
                {
                    success = true,
                    message = $"Se han enviado recordatorios a {recordatoriosEnviados} estudiantes con documentos pendientes."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error al enviar recordatorios: {ex.Message}",
                    details = ex.InnerException?.Message
                });
            }
        }

        #region Llamadas a la API
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.DocumentCatalog.GetAll() });
        }
        [HttpGet]
        public IActionResult VerifyUniqueName(string name)
        {
            var existDocument = _contenedorTrabajo.DocumentCatalog.GetAll()
                                    .Any(d => d.Name.ToLower() == name.ToLower());

            if (existDocument)
            {
                // Si el nombre ya existe, retorna un error
                return Json($"Ya existe un documento con el nombre '{name}'.");
            }

            // Si el nombre es único, retorna true (sin errores)
            return Json(true);
        }
        public IActionResult Delete(int id)
        {
            var objFromDb = _contenedorTrabajo.DocumentCatalog.GetById(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error borrando documento" });
            }

            _contenedorTrabajo.DocumentCatalog.Remove(objFromDb);
            _contenedorTrabajo.Save();
            return Json(new { success = true, message = "Documento borrado correctamente" });
        }

        #endregion
    }
}
