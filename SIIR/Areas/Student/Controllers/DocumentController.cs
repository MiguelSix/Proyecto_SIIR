using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    //[Authorize(Roles = "Student")]
    public class DocumentController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        public DocumentController(IContenedorTrabajo contenedorTrabajo)
        {
            _contenedorTrabajo = contenedorTrabajo;
        }
        [HttpGet]
        public IActionResult Index()
        {
            // Mapeamos DocumentCatalog a SelectListItem
            var listaDocumentos = _contenedorTrabajo.DocumentCatalog.GetAll().Select(doc => new SelectListItem
            {
                Text = doc.Name, // Nombre del documento
                Value = doc.Id.ToString(), // Id del documento
                Group = new SelectListGroup { Name = doc.Extension }, // Agrupamos por tipo (ejemplo: pdf, foto, etc.)
            });

            DocumentVM docuVM = new DocumentVM()
            {
                Document = new SIIR.Models.Document(),
                ListaDocumenCatalog = listaDocumentos // Aquí pasamos la lista como SelectListItem
            };

            return View(docuVM);
        }
        // Acción GET para cargar la vista de edición
        [HttpGet]
        public IActionResult Edit()
        {
            var listaDocumentos = _contenedorTrabajo.DocumentCatalog.GetDocumentCatalogList();
            return View(listaDocumentos); // Pasamos la lista a la vista Edit
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
