using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIIR.Data;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;

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
                foreach (var uniformCatalogId in representativeVM.SelectedUniformCatalogIds)
                {
                    var representativeUniformCatalog = new RepresentativeUniformCatalog
                    {
                        RepresentativeId = representativeVM.Representative.Id,
                        UniformCatalogId = uniformCatalogId
                    };
                    _contenedorTrabajo.RepresentativeUniformCatalog.Add(representativeUniformCatalog);
                    
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
            representativeVM.Representative = _contenedorTrabajo.Representative
                .GetAll(r => r.Id == id)
                .FirstOrDefault();
            representativeVM.SelectedUniformCatalogIds = _contenedorTrabajo.RepresentativeUniformCatalog
                .GetAll(ruc => ruc.RepresentativeId == id)
                .Select(ruc => ruc.UniformCatalogId)
                .ToList();
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
            var existingRUCs = _contenedorTrabajo.RepresentativeUniformCatalog
                .GetAll(ruc => ruc.RepresentativeId == representativeVM.Representative.Id)
                .ToList();

            if (representativeVM.SelectedUniformCatalogIds is null)
            {
                foreach (var existingRUC in existingRUCs)
                {
                    _contenedorTrabajo.RepresentativeUniformCatalog.Remove(existingRUC);
                }
            }
            else 
            { 
                foreach (var existingRUC in existingRUCs)
                {
                    if (!representativeVM.SelectedUniformCatalogIds.Contains(existingRUC.UniformCatalogId))
                    {
                        _contenedorTrabajo.RepresentativeUniformCatalog.Remove(existingRUC);
                    }
                }

                // Add new entries
                foreach (var uniformCatalogId in representativeVM.SelectedUniformCatalogIds)
                {
                    if (!existingRUCs.Any(ruc => ruc.UniformCatalogId == uniformCatalogId))
                    {
                        var newRUC = new RepresentativeUniformCatalog
                        {
                            RepresentativeId = representativeVM.Representative.Id,
                            UniformCatalogId = uniformCatalogId
                        };
                        _contenedorTrabajo.RepresentativeUniformCatalog.Add(newRUC);
                    }
                }
            }
            _contenedorTrabajo.Save();
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Representative.GetAll() });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
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
