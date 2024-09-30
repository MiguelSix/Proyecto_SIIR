using Microsoft.AspNetCore.Mvc;
using SIIR.DataAccess.Data.Repository.IRepository;

namespace SIIR.Areas.Coach.Controllers
{
    [Area("Coach")]
    public class CoachController : Controller
    {
        //Se utiliza el contenedor de trabajo de Coach para almacenar los datos y utilizarlos
        /*private readonly IContenedorTrabajo _contenedorTrabajo;

        public CoachController(IContenedorTrabajo contenedorTrabajo)
        {
            _contenedorTrabajo = contenedorTrabajo;
        }*/
        
        //Se utiliza HttpGet por que se van a traer y utilizar los datos del coach para acceso a vistas y asi?
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
