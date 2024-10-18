using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIIR.Data;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RepresentativesController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;

        public RepresentativesController(IContenedorTrabajo contenedorTrabajo)
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
            RepresentativeVM representativeVM = new RepresentativeVM()
            {
                Representative = new Representative(),
                UniformCatalogList = _contenedorTrabajo.UniformCatalog.GetUniformCatalogList()
            };
            return View(representativeVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RepresentativeVM representativeVM)
        {
            if (ModelState.IsValid)
            {
                _contenedorTrabajo.Representative.Add(representativeVM.Representative);
                _contenedorTrabajo.Save();
                CreateRepresentativeUniformCatalog(representativeVM);
                return RedirectToAction(nameof(Index));
            }
            representativeVM.UniformCatalogList = _contenedorTrabajo.UniformCatalog.GetUniformCatalogList();
            return View(representativeVM);
        }

        private void CreateRepresentativeUniformCatalog(RepresentativeVM representativeVM)
        {
            if (representativeVM.SelectedUniformCatalogIds != null)
            {
                var representative = representativeVM.Representative;
                if (representative.UniformCatalogs == null)
                {
                    representative.UniformCatalogs = new List<UniformCatalog>();
                }


                foreach (var uniformCatalogId in representativeVM.SelectedUniformCatalogIds)
                {
                    var uniformCatalog = _contenedorTrabajo.UniformCatalog.GetById(uniformCatalogId);
                    representative.UniformCatalogs.Add(uniformCatalog);
                }
                _contenedorTrabajo.Save();
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            RepresentativeVM representativeVM = new RepresentativeVM()
            {
                Representative = new Representative(),
                UniformCatalogList = _contenedorTrabajo.UniformCatalog.GetUniformCatalogList()
            };
            if (id != null)
            {
                representativeVM.Representative = _contenedorTrabajo.Representative
                .GetAll(r => r.Id == id, includeProperties: "UniformCatalogs")
                .FirstOrDefault();

            }
            return View(representativeVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RepresentativeVM representativeVM)
        {
            if (ModelState.IsValid)
            {
                _contenedorTrabajo.Representative.Update(representativeVM.Representative);
                _contenedorTrabajo.Save();
                UpdateRepresentativeUniformCatalog(representativeVM);
                return RedirectToAction(nameof(Index));
            }
            representativeVM.UniformCatalogList = _contenedorTrabajo.UniformCatalog.GetUniformCatalogList();
            return View(representativeVM);
        }

        private void UpdateRepresentativeUniformCatalog(RepresentativeVM representativeVM)
        {
                var representative = representativeVM.Representative;

                var existingUniformCatalogs = _contenedorTrabajo.Representative
                    .GetAll(r => r.Id == representative.Id, includeProperties: "UniformCatalogs")
                    .FirstOrDefault()?.UniformCatalogs ?? new List<UniformCatalog>();

                representative.UniformCatalogs = existingUniformCatalogs;
                var selectedUniformCatalogs = new HashSet<int>(representativeVM.SelectedUniformCatalogIds ?? new List<int>());
                var existingUniformCatalogIds = new HashSet<int>(existingUniformCatalogs.Select(u => u.Id));

                var catalogsToRemove = existingUniformCatalogIds.Except(selectedUniformCatalogs).ToList();
                foreach (var catalogId in catalogsToRemove)
                {
                    var catalogToRemove = existingUniformCatalogs.FirstOrDefault(c => c.Id == catalogId);
                    if (catalogToRemove != null)
                    {
                        representative.UniformCatalogs.Remove(catalogToRemove);
                    }
                }

                var catalogsToAdd = selectedUniformCatalogs.Except(existingUniformCatalogIds).ToList();
                foreach (var catalogId in catalogsToAdd)
                {
                    var catalogToAdd = _contenedorTrabajo.UniformCatalog.GetById(catalogId);
                    if (catalogToAdd != null)
                    {
                        representative.UniformCatalogs.Add(catalogToAdd);
                    }
                }

                _contenedorTrabajo.Save();
        }

        #region

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Representative.GetAll() });
        }

        [HttpDelete]
        public IActionResult Delete(int id) {
            var objFromDb = _contenedorTrabajo.Representative.GetById(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error al borrar el grupo representativo." });
            }

            // Verificar si hay equipos asociados a este representativo
            var teamsAssociated = _contenedorTrabajo.Team.GetAll(t => t.RepresentativeId == id);

            if (teamsAssociated.Any())
            {
                // Si hay equipos asociados, no permitimos el borrado
                return Json(new { success = false, message = "No se puede borrar el grupo representativo porque hay equipos asociados a él." });
            }

            // Si no hay equipos asociados, procedemos con el borrado
            _contenedorTrabajo.Representative.Remove(objFromDb);
            _contenedorTrabajo.Save();

            return Json(new { success = true, message = "Grupo representativo borrado exitosamente." });
        }

        #endregion

    }
}
