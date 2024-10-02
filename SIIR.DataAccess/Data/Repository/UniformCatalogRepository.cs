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
                objFromDb.Description = uniformCatalog.Description;
                _db.SaveChanges();
            }
        }

		public IEnumerable<SelectListItem> GetUniformCatalogList()
		{
			return _db.UniformCatalog.Select(i => new SelectListItem
			{
				Text = i.Name,
				Value = i.Id.ToString()
			});
		}
	}
}
