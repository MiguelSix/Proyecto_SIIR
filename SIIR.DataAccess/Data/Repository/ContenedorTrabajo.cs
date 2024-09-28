using SIIR.Data;
using SIIR.DataAccess.Data.Repository;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.DataAccess.Data.Repository
{
    public class ContenedorTrabajo : IContenedorTrabajo
    {
        private readonly ApplicationDbContext _db;
        public ContenedorTrabajo(ApplicationDbContext db)
        {
            _db = db;
            User = new UserRepository(_db);
            Team = new TeamRepository(_db);
            Representative = new RepresentativeRepository(_db);
            UniformCatalog = new UniformCatalogRepository(_db);
        }

        public IUserRepository User { get; private set; }
        public ITeamRepository Team { get; private set; }
        public IRepresentativeRepository Representative { get; private set; }
        public IUniformCatalogRepository UniformCatalog { get; private set; }

        public void Dispose()
        {
            _db.Dispose();
        }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
