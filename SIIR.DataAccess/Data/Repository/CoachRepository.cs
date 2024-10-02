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
    public class CoachRepository: Repository<Coach>, ICoachRepository
    {
        private readonly ApplicationDbContext _db;
        public CoachRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public IEnumerable<SelectListItem> GetCoachesList()
        {
            return _db.Coaches.Select(i => new SelectListItem()
            {
                Text = i.Name + " " + i.LastName,
                Value = i.Id.ToString()
            });
        }

        public void Update(Coach coach)
        {
            var objDesdeDb = _db.Coaches.FirstOrDefault(i => i.Id == coach.Id);
            objDesdeDb.Name = coach.Name;
            objDesdeDb.LastName = coach.LastName;
            objDesdeDb.SecondLastName = coach.SecondLastName;

            _db.SaveChanges();
        }
    }
}
