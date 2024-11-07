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

        public HomeController(IContenedorTrabajo contenedorTrabajo, UserManager<ApplicationUser> userManager)
        {
            _contenedorTrabajo = contenedorTrabajo;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task <IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ApplicationUser user = await _userManager.FindByIdAsync(userId); 
            Models.Coach coach = new Models.Coach();
            coach = _contenedorTrabajo.Coach.GetById((int) user.CoachId);
            return View(coach);
        }

        [HttpGet]
        public IActionResult GetAllTeams(string genderCategories = null, string groupCategories = null, string searchTerm = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ApplicationUser user = _userManager.FindByIdAsync(userId).Result;
            int coachId = (int)user.CoachId;

            List<string> genderList = !string.IsNullOrEmpty(genderCategories) ?
                JsonConvert.DeserializeObject<List<string>>(genderCategories) : new List<string>();

            List<string> groupList = !string.IsNullOrEmpty(groupCategories) ?
                JsonConvert.DeserializeObject<List<string>>(groupCategories) : new List<string>();

            // Filtrar los equipos con las listas de filtros y el término de búsqueda
            var equipos = _contenedorTrabajo.Team.GetAll(t =>
                t.CoachId == coachId &&
                (genderList.Count == 0 || genderList.Contains(t.Category)) &&
                (groupList.Count == 0 || groupList.Contains(t.Representative.Category)) &&
                (string.IsNullOrEmpty(searchTerm) || t.Name.ToLower().Contains(searchTerm.ToLower())), // Filtrar por nombre (LIKE)
                includeProperties: "Representative");

            return Json(new { data = equipos });
        }

        public IActionResult GetStudentsByTeamId(int teamId)
        {
            var users = _contenedorTrabajo.User.GetAll(u => u.LockoutEnd == null && u.StudentId != null).Select(u => u.StudentId).ToList();
            // Lista de estudiantes que pertenecen al equipo y no están bloqueados como usuarios
            var students = _contenedorTrabajo.Student.GetAll(s => s.TeamId == teamId && users.Contains(s.Id));
            return Json(new { data = students });
        }
    }
}
