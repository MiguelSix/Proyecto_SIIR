using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Newtonsoft.Json;
using SIIR.Data;
using SIIR.DataAccess.Data.Repository;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using System.Security.Claims;

namespace SIIR.Areas.Coach.Controllers
{
    [Area("Coach")]
    [Authorize(Roles = "Coach")]
    public class HomeController : Controller
    {

        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public HomeController(
               IContenedorTrabajo contenedorTrabajo, 
               UserManager<ApplicationUser> userManager,
               IWebHostEnvironment hostingEnvironment)
        {
            _contenedorTrabajo = contenedorTrabajo;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public async Task <IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            { 
                return NotFound();
            }
            ApplicationUser user = await _userManager.FindByIdAsync(userId); 
            Models.Coach coach = new Models.Coach();
            if (user.CoachId == null)
            { 
                return NotFound();
            }
            coach = _contenedorTrabajo.Coach.GetById((int) user.CoachId);
            return View(coach);
        }

        #region API_CALLS

        [HttpGet]
        public IActionResult GetAllTeams(int coachId)
        {
            var equipos = _contenedorTrabajo.Team.GetAll(t =>
                t.CoachId == coachId,
                includeProperties: "Representative");

            return Json(new { data = equipos });
        }

        public IActionResult GetStudentsByTeamId(int teamId)
        {
            var users = _contenedorTrabajo.User.GetAll(u => u.LockoutEnd == null && u.StudentId != null).Select(u => u.StudentId).ToList();
            var students = _contenedorTrabajo.Student.GetAll(s => s.TeamId == teamId && users.Contains(s.Id));
            return Json(new { data = students });
        }

        #endregion
    }
}
