using Microsoft.AspNetCore.Mvc.Rendering;
using SIIR.Data;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.DataAccess.Data.Repository
{
    public class AdminRepository: Repository<Admin>, IAdminRepository
    {
        private readonly ApplicationDbContext _db;
        public AdminRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<SelectListItem> GetAdminsList()
        {
            return _db.Admins.Select(i => new SelectListItem()
            {
                Text = i.Name + " " + i.LastName + " " + i.SecondLastName,
                Value = i.Id.ToString()
            });
        }
    }
}
