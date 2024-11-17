using Microsoft.AspNetCore.Mvc.Rendering;
using SIIR.Data;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;


namespace SIIR.DataAccess.Data.Repository
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        private readonly ApplicationDbContext _db;

        public NotificationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<Notification> GetByUserId(int userId)
        {
            return _db.Notification
                .Where(n => n.Student.Id == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        public int GetUnreadCount(int userId)
        {
            return _db.Notification
                .Count(n => n.Student.Id == userId && !n.IsRead);
        }

        public IEnumerable<Notification> GetLatestNotifications(int userId, int count)
        {
            return _db.Notification
                .Where(n => n.Student.Id == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToList();
        }

        public IEnumerable<Notification> GetUnreadNotifications(int userId)
        {
            return _db.Notification
                .Where(n => n.Student.Id == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        public void MarkAsRead(int id)
        {
            var notification = _db.Notification.Find(id);
            if (notification != null)
            {
                notification.IsRead = true;
                _db.SaveChanges();
            }
        }

        public void MarkAllAsRead(int userId)
        {
            var unreadNotifications = _db.Notification
                .Where(n => n.Student.Id == userId && !n.IsRead);

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            _db.SaveChanges();
        }

        public void DeleteOldNotifications(int daysOld)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldNotifications = _db.Notification
                .Where(n => n.CreatedAt < cutoffDate);

            _db.Notification.RemoveRange(oldNotifications);
            _db.SaveChanges();
        }

        public bool HasUnreadNotifications(int userId)
        {
            return _db.Notification
                .Any(n => n.Student.Id == userId && !n.IsRead);
        }

        public IEnumerable<SelectListItem> GetNotificationsList()
        {
            return _db.Notification.Select(i => new SelectListItem()
            {
                Text = $"{i.Student.Name} - {i.Type} - {i.CreatedAt:dd/MM/yyyy}",
                Value = i.Id.ToString()
            });
        }

        public void Update(Notification notification)
        {
            var objFromDb = _db.Notification.FirstOrDefault(s => s.Id == notification.Id);
            if (objFromDb != null)
            {
                objFromDb.Message = notification.Message;
                objFromDb.IsRead = notification.IsRead;
                objFromDb.Type = notification.Type;
                objFromDb.DocumentId = notification.DocumentId;
                objFromDb.StudentId = notification.StudentId;

                _db.SaveChanges();
            }
        }
    }
}

