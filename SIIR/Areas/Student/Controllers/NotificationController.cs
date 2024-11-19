using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIIR.Data;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;

namespace SIIR.Areas.Student.Controllers
{
    [Area("Student")]
    public class NotificationController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(
            IContenedorTrabajo contenedorTrabajo,
            UserManager<ApplicationUser> userManager)
        {
            _contenedorTrabajo = contenedorTrabajo;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { count = 0 });
            }

            // Si el usuario es estudiante
            if (user.StudentId.HasValue)
            {
                var count = _contenedorTrabajo.Notification
                    .GetAll(n => n.StudentId == user.StudentId && !n.IsRead)
                    .Count();

                return Json(new { count });
            }

            return Json(new { count = 0 });
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new List<Notification>());
            }

            // Si el usuario es estudiante
            if (user.StudentId.HasValue)
            {
                var notifications = _contenedorTrabajo.Notification
                    .GetAll(n => n.StudentId == user.StudentId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5)
                    .ToList();

                return Json(notifications);
            }

            return Json(new List<Notification>());
        }
        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            var notification = _contenedorTrabajo.Notification.GetFirstOrDefault(n => n.Id == id);

            if (notification != null)
            {
                notification.IsRead = true;
                _contenedorTrabajo.Notification.Update(notification);
                _contenedorTrabajo.Save();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View(new List<Notification>());
            }

            // Si el usuario es estudiante
            if (user.StudentId.HasValue)
            {
                var notifications = _contenedorTrabajo.Notification
                    .GetAll(n => n.StudentId == user.StudentId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();

                return View(notifications);
            }

            return View(new List<Notification>());
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || !user.StudentId.HasValue)
                {
                    return Json(new { success = false, message = "Usuario no autorizado" });
                }

                // Buscar la notificación asegurándonos que pertenece al estudiante actual
                var notification = _contenedorTrabajo.Notification
                    .GetFirstOrDefault(n => n.Id == id && n.StudentId == user.StudentId);

                if (notification == null)
                {
                    return Json(new { success = false, message = "Notificación no encontrada" });
                }

                // Eliminar la notificación
                _contenedorTrabajo.Notification.Remove(notification);
                _contenedorTrabajo.Save();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Podrías agregar logging aquí si lo tienes configurado
                return Json(new { success = false, message = "Error al eliminar la notificación" });
            }
        }
    }
}
