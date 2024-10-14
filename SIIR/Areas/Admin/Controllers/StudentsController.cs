using Microsoft.AspNetCore.Mvc;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StudentsController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public StudentsController(IContenedorTrabajo contenedorTrabajo,
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
        public IActionResult Edit(int id) 
        {
            var student = _contenedorTrabajo.Student.GetFirstOrDefault(
                s => s.Id == id,
                includeProperties: "Team,Coach"
            );

            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.Student student)
        {
            // Quitar Coach y Team del ModelState
            ModelState.Remove("Coach");
            ModelState.Remove("Team");

            if (ModelState.IsValid)
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                var studentFromDb = _contenedorTrabajo.Student.GetById(student.Id);

                if (studentFromDb == null)
                {
                    return NotFound();
                }

                if (files.Count > 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\students");
                    var extension = Path.GetExtension(files[0].FileName);

                    if (!string.IsNullOrEmpty(studentFromDb.ImageUrl))
                    {
                        var imagePath = Path.Combine(webRootPath, studentFromDb.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    student.ImageUrl = @"\images\students\" + fileName + extension;
                }
                else
                {
                    student.ImageUrl = studentFromDb.ImageUrl;
                }

                _contenedorTrabajo.Student.Update(student);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Student.GetAll(includeProperties: "Team,Coach") });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _contenedorTrabajo.Student.GetById(id);
            //string webRootPath = _hostingEnvironment.WebRootPath;
            //var imagePath = Path.Combine(webRootPath, objFromDb.ImageUrl.TrimStart('\\'));

            //if (System.IO.File.Exists(imagePath))
            //{
            //    System.IO.File.Delete(imagePath);
            //}

            //if (objFromDb == null)
            //{
            //    return Json(new { success = false, message = "Error while deleting." });
            //}

            _contenedorTrabajo.Student.Remove(objFromDb);
            _contenedorTrabajo.Save();
            return Json(new { success = true, message = "Delete successful." });
        }

        #endregion
    }
}
