using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
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
    }
}
