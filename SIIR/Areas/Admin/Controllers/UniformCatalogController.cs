using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UniformCatalogController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public UniformCatalogController(IContenedorTrabajo contenedorTrabajo, IWebHostEnvironment hostingEnvironment)
        {
            _contenedorTrabajo = contenedorTrabajo;
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UniformCatalog uniformCatalog)
        {
			var existUniform = _contenedorTrabajo.UniformCatalog.GetAll()
									.Any(d => d.Name.ToLower() == uniformCatalog.Name.ToLower()
										   && d.Id != uniformCatalog.Id);


			if (existUniform)
			{
				ModelState.AddModelError("Nombre", "Ya existe un uniforme del catalogo con este nombre.");
			}

			if (ModelState.IsValid)
            {
                _contenedorTrabajo.UniformCatalog.Add(uniformCatalog);
                _contenedorTrabajo.Save();

                return RedirectToAction(nameof(Index)); 
            }

            return View(uniformCatalog);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {

			UniformCatalog uniformCatalog = _contenedorTrabajo.UniformCatalog.GetById(id);
			if (uniformCatalog == null)
			{
				return NotFound();
			}


			return View(uniformCatalog);
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UniformCatalog uniformCatalog)
        {
			var existUniform = _contenedorTrabajo.UniformCatalog.GetAll()
									.Any(d => d.Name.ToLower() == uniformCatalog.Name.ToLower()
										   && d.Id != uniformCatalog.Id); 


			if (existUniform)
			{
				ModelState.AddModelError("Nombre", "Ya existe un uniforme del catalogo con este nombre.");
			}

			if (ModelState.IsValid)
			{
				_contenedorTrabajo.UniformCatalog.Update(uniformCatalog);
				_contenedorTrabajo.Save();
				return RedirectToAction(nameof(Index));
			}

            return View(uniformCatalog);
		}

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _contenedorTrabajo.UniformCatalog.GetById(id);

            _contenedorTrabajo.UniformCatalog.Remove(objFromDb);
            _contenedorTrabajo.Save();
            return Json(new { success = true, message = "Uniforme borrada exitosamente" });
        }

        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.UniformCatalog.GetAll() });
        }
    }
}
