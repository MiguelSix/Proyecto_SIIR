using Microsoft.AspNetCore.Mvc.Rendering;
using SIIR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.DataAccess.Data.Repository.IRepository
{
    public interface INotificationRepository : IRepository<Notification>
    {
        IEnumerable<SelectListItem> GetNotificationsList();
        IEnumerable<Notification> GetByUserId(int userId);
        int GetUnreadCount(int userId);
        IEnumerable<Notification> GetLatestNotifications(int userId, int count);
        IEnumerable<Notification> GetUnreadNotifications(int userId);
        void MarkAsRead(int id);
        void MarkAllAsRead(int userId);
        void DeleteOldNotifications(int daysOld);
        bool HasUnreadNotifications(int userId);
        void Update(Notification notification);
    }
}
