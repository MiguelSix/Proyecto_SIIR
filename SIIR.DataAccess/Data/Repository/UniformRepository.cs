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
    public class UniformRepository : Repository<Uniform>, IUniformRepository
    {
        private readonly ApplicationDbContext _db;

        public UniformRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Uniform uniform)
        {
            var objDesdeDb = _db.Uniform.FirstOrDefault(u => u.Id == uniform.Id);
            if (objDesdeDb != null)
            {
                objDesdeDb.size = uniform.size;
            }
        }

    }
}
