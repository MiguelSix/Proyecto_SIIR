using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIIR.DataAccess.Data.Repository.IRepository;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        public UsersController(IContenedorTrabajo contenedorTrabajo)
        {
            _contenedorTrabajo = contenedorTrabajo;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(_contenedorTrabajo.User.GetAll());
        }

        [HttpGet]
        public IActionResult Lock(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            _contenedorTrabajo.User.LockUser(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Unlock(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            _contenedorTrabajo.User.UnlockUser(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
