using Microsoft.AspNetCore.Mvc;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models.ViewModels;

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
        public IActionResult Create()
        {
            StudentVM studentVM = new()
            {
                Student = new(),
                TeamList = _contenedorTrabajo.Team.GetListaTeams(),
            };
            return View(studentVM);
        }

        [HttpGet]
        public IActionResult Edit(int? id) 
        {
            StudentVM studentVM = new()
            {
                Student = new(),
                TeamList = _contenedorTrabajo.Team.GetListaTeams(),
            };
            if (id != null)
            {
                studentVM.Student = _contenedorTrabajo.Student.GetById(id.GetValueOrDefault());
            }
            return View(studentVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StudentVM studentVM)
        {
            return View();
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
