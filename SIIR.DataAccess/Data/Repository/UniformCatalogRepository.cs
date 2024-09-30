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
    internal class UniformCatalogRepository : Repository<UniformCatalog>, IUniformCatalogRepository
    {
        private readonly ApplicationDbContext _db;

        public UniformCatalogRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(UniformCatalog uniformCatalog)
        {
            var objFromDb = _db.UniformCatalog.FirstOrDefault(s => s.Id == uniformCatalog.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = uniformCatalog.Name;
                objFromDb.HasNumber = uniformCatalog.HasNumber;
                objFromDb.RepresentativeId = uniformCatalog.RepresentativeId;
                _db.SaveChanges();
            }
        }
    }
}
