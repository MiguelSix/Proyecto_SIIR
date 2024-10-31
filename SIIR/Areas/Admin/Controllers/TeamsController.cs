using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TeamsController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public TeamsController(IContenedorTrabajo contenedorTrabajo,
            IWebHostEnvironment hostingEnvironment)
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
            TeamVM teamVM = new()
            {
                Team = new(),
                RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList(),
                CoachList = _contenedorTrabajo.Coach.GetCoachesList(),
            };
            return View(teamVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TeamVM teamVM)
        {
            if (!ModelState.IsValid)
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (teamVM.Team.Id == 0 && files.Count() > 0)
                {
                    // Nuevo equipo
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\teams");
                    var extension = Path.GetExtension(files[0].FileName);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStreams);
                    }

                    teamVM.Team.ImageUrl = @"\images\teams\" + fileName + extension;
                    _contenedorTrabajo.Team.Add(teamVM.Team);
                    _contenedorTrabajo.Save();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("Imagen", "Debes seleccionar una imagen");
                }
            }
            teamVM.RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList();
            teamVM.CoachList = _contenedorTrabajo.Coach.GetCoachesList();

			return View(teamVM);
        }


        [HttpGet]
        public IActionResult Edit(int? id)
        {
            TeamVM teamVM = new()
            {
                Team = new(),
                RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList(),
                CoachList = _contenedorTrabajo.Coach.GetCoachesList(),
            };
            if (id != null)
            {
                teamVM.Team = _contenedorTrabajo.Team.GetById(id.GetValueOrDefault());
            }
            return View(teamVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TeamVM teamVM)
        {
            // Remover la validación de la imagen si no se selecciona una nueva imagen
            ModelState.Remove("Team.ImageUrl");
            if (ModelState.IsValid)
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                var teamFromDb = _contenedorTrabajo.Team.GetById(teamVM.Team.Id);

                if (files.Count > 0)
                {
                    // Editar Imagen
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\teams");
                    var extension_new = Path.GetExtension(files[0].FileName);

                    // Eliminar la imagen anterior
                    if (!string.IsNullOrEmpty(teamFromDb.ImageUrl))
                    {
                        var imagePath = Path.Combine(webRootPath, teamFromDb.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    // Subir nueva imagen
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension_new), FileMode.Create))
                    {
                        files[0].CopyTo(fileStreams);
                    }

                    // Guardar la nueva ruta de la imagen
                    teamFromDb.ImageUrl = @"\images\teams\" + fileName + extension_new;
                }
                else
                {
                    // No se seleccionó una nueva imagen, mantener la imagen existente
                    teamVM.Team.ImageUrl = teamFromDb.ImageUrl;
                }

                _contenedorTrabajo.Team.Update(teamVM.Team);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }

            // Si el ModelState no es válido, volver a cargar las listas y retornar la vista
            teamVM.RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList();
            teamVM.CoachList = _contenedorTrabajo.Coach.GetCoachesList();
            return View(teamVM);
        }

        [HttpGet]
        public IActionResult Roster(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = _contenedorTrabajo.Team.GetById(id.Value);
            if (team == null)
            {
                return NotFound();
            }
            // Data del equipo
            team.Coach = _contenedorTrabajo.Coach.GetById(team.CoachId);
            var students = _contenedorTrabajo.Student.GetAll(s => s.TeamId == id.Value);
            var captain = students.FirstOrDefault(s => s.IsCaptain);

            TeamVM teamVM = new()
            {
                Team = team,
                StudentList = students.Select(s => new SelectListItem
                {
                    Text = $"{s.Name} {s.LastName} {s.SecondLastName}",
                    Value = s.Id.ToString()
                }),
                Captain = captain
            };

            return View(teamVM);
        }

        [HttpPost]
        public IActionResult ChangeCaptain(int teamId, int newCaptainId)
        {
            try
            {
                // Get current captain if exists
                var currentCaptain = _contenedorTrabajo.Student.GetAll(s => s.TeamId == teamId && s.IsCaptain).FirstOrDefault();

                // Remove captain status from current captain
                if (currentCaptain != null)
                {
                    currentCaptain.IsCaptain = false;
                    _contenedorTrabajo.Student.Update(currentCaptain);
                }  

                // Set new captain
                var newCaptain = _contenedorTrabajo.Student.GetById(newCaptainId);
                if (newCaptain == null || newCaptain.TeamId != teamId)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado o no pertenece al equipo" });
                }

                newCaptain.IsCaptain = true;
                _contenedorTrabajo.Student.Update(newCaptain);
                _contenedorTrabajo.Save();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cambiar el capitán: " + ex.Message });
            }
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Team.GetAll(includeProperties: "Representative,Coach") });
        }

        [HttpGet]
        public IActionResult GetStudentsByTeamId(int teamId)
        {
            // Obtener los estudiantes por equipo
            var students = _contenedorTrabajo.Student.GetAll(s => s.TeamId == teamId);
            return Json(new { data = students });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _contenedorTrabajo.Team.GetById(id);

            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error al borrar el equipo" });
            }

            // Verificar si el equipo tiene estudiantes asignados
            var teamWithStudents = _contenedorTrabajo.Student.GetAll(s => s.TeamId == id);

            //Si el equipo tiene alumnos asignados, no se puede borrar  
            if (teamWithStudents.Any())
            {
                return Json(new { success = false, message = "No se puede borrar el equipo porque tiene alumnos asignados" });
            }

            // Si llegamos aquí, podemos borrar la imagen y el equipo
            string webRootPath = _hostingEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, objFromDb.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _contenedorTrabajo.Team.Remove(objFromDb);
            _contenedorTrabajo.Save();
            return Json(new { success = true, message = "Equipo borrado exitosamente" });
        }

        #endregion
    }
}
