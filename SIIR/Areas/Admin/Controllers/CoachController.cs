using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CoachController : Controller
    {
        //Se utiliza el contenedor de trabajo de Coach para almacenar los datos y utilizarlos
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public CoachController(IContenedorTrabajo contenedorTrabajo, IWebHostEnvironment hostingEnvironment)
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
        public IActionResult Edit(int id)
        {
            var coach = _contenedorTrabajo.Coach.GetById(id);
            
            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.Coach coach)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    var files = HttpContext.Request.Form.Files;
                    var coachFromDb = _contenedorTrabajo.Coach.GetById(coach.Id);

                    if (coachFromDb == null)
                    {
                        return NotFound();
                    }

                    if (files.Count > 0)
                    {
                        var file = files[0];
                        if (file.Length > 0)
                        {
                            // Crear el directorio si no existe
                            var uploadsFolder = Path.Combine(webRootPath, "images", "coaches");
                            Directory.CreateDirectory(uploadsFolder);

                            // Generar nombre único para el archivo
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string filePath = Path.Combine(uploadsFolder, fileName);

                            // Eliminar imagen anterior
                            if (!string.IsNullOrEmpty(coachFromDb.ImageUrl))
                            {
                                var oldImagePath = Path.Combine(webRootPath, coachFromDb.ImageUrl.TrimStart('/'));
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                            }

                            // Guardar nueva imagen
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                file.CopyTo(fileStream);
                            }

                            // Actualizar la URL en el modelo
                            coach.ImageUrl = "/images/coaches/" + fileName;
                        }
                    }
                    else
                    {
                        // Mantener la imagen existente
                        coach.ImageUrl = coachFromDb.ImageUrl;
                    }

                    // Actualizar otros campos del coach
                    coachFromDb.Name = coach.Name;
                    coachFromDb.LastName = coach.LastName;
                    coachFromDb.SecondLastName = coach.SecondLastName;
                    coachFromDb.ImageUrl = coach.ImageUrl;

                    _contenedorTrabajo.Coach.Update(coachFromDb);
                    _contenedorTrabajo.Save();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log the exception
                    ModelState.AddModelError("", "Ha ocurrido un error al actualizar el coach: " + ex.Message);
                }
            }

            return View(coach);
        }


        #region Llamadas a la API

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Coach.GetAll() });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _contenedorTrabajo.Coach.GetById(id);

            var user = _contenedorTrabajo.User.GetAll(u => u.CoachId == id).FirstOrDefault();

            if (user != null)
            {
                _contenedorTrabajo.User.Remove(user);
            }

            if (objFromDb == null)
            {
                return Json(new { succes = false, message = "Error al borrar Coach"});
            }

            _contenedorTrabajo.Coach.Remove(objFromDb);
            _contenedorTrabajo.Save();
            return Json(new { succes = true, message = "Exito al borrar Coach" });
        }
        #endregion
    }
}
