using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using SIIR.DataAccess.Data.Repository.IRepository;
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
            UniformCatalogVM UniformVM = new UniformCatalogVM()
            {
                UniformCatalog = new Models.UniformCatalog(),
                RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList()
            };

            return View(UniformVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UniformCatalogVM uniformVM)
        {
            if (ModelState.IsValid)
            {
                _contenedorTrabajo.UniformCatalog.Add(uniformVM.UniformCatalog);
                _contenedorTrabajo.Save();

                return RedirectToAction(nameof(Index)); 
            }

            uniformVM.RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList();

            return View(uniformVM);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            UniformCatalogVM uniformVM = new UniformCatalogVM()
            {
                UniformCatalog = new Models.UniformCatalog(),
                RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList()
            };

            if (id != null)
            {
                uniformVM.UniformCatalog = _contenedorTrabajo.UniformCatalog.GetById(id.GetValueOrDefault());
            }

            return View(uniformVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UniformCatalogVM uniformVM)
        {
            if (ModelState.IsValid)
            {
                var UniformCatalogDesdeDb = _contenedorTrabajo.UniformCatalog.GetById(uniformVM.UniformCatalog.Id);
                _contenedorTrabajo.UniformCatalog.Update(uniformVM.UniformCatalog);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }

            uniformVM.RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList();
            return View(uniformVM);
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
            return Json(new { data = _contenedorTrabajo.UniformCatalog.GetAll(includeProperties: "Representative") });
        }
    }
}
