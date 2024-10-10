using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DocumentCatalogController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        public DocumentCatalogController(IContenedorTrabajo contenedorTrabajo)
        {
            _contenedorTrabajo = contenedorTrabajo;
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
