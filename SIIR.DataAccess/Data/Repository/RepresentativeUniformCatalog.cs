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
    public class RepresentativeUniformCatalogRepository : Repository<RepresentativeUniformCatalog>, IRepresentativeUniformCatalog
    {
        private readonly ApplicationDbContext _db;

        public RepresentativeUniformCatalogRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(RepresentativeUniformCatalog representativeUniformCatalog)
        {
            var objDesdeDb = _db.RepresentativeUniformCatalogs
                               .FirstOrDefault(ruc => ruc.RepresentativeId == representativeUniformCatalog.RepresentativeId
                               && ruc.UniformCatalogId == representativeUniformCatalog.UniformCatalogId);
            _db.SaveChanges();
        }
    }
}
