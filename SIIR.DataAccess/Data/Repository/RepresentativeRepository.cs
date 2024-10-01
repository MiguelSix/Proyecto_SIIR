using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
    public class RepresentativeRepository : Repository<Representative>, IRepresentativeRepository
    {
        private readonly ApplicationDbContext _db;

        public RepresentativeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<SelectListItem> GetRepresentativesList()
        {
            return _db.Representatives.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
        }

        public void Update(Representative representative)
        {
            var objFromDb = _db.Representatives.FirstOrDefault(s => s.Id == representative.Id);
            objFromDb.Name = representative.Name;
            objFromDb.Category = representative.Category;
            objFromDb.UniformCatalogs = representative.UniformCatalogs;

            _db.SaveChanges();
        }
    }
}
