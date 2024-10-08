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
    public class TeamRepository: Repository<Team>, ITeamRepository
    {
        private readonly ApplicationDbContext _db;

        public TeamRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<SelectListItem> GetListaCoaches()
        {
            return _db.Coaches.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
        }

        public IEnumerable<SelectListItem> GetListaRepresentatives()
        {
            return _db.Representatives.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
        }

        public IEnumerable<SelectListItem> GetListaStudents()
        {
            return _db.Students.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
        }

        public IEnumerable<SelectListItem> GetListaTeams()
        {
            return _db.Teams.Select(i => new SelectListItem
            {
                Text = i.Name + " " + i.Category,
                Value = i.Id.ToString()
            });
        }

        public void Update(Team team)
        {
            var objDesdeDb = _db.Teams.FirstOrDefault(s => s.Id == team.Id);

            objDesdeDb.Name = team.Name;
            objDesdeDb.Category = team.Category;
            objDesdeDb.CoachId = team.CoachId;
            objDesdeDb.RepresentativeId = team.RepresentativeId;
            objDesdeDb.StudentId = team.StudentId;

            _db.SaveChanges();
        }
    }
}
