using Microsoft.AspNetCore.Mvc.Rendering;
using SIIR.Data;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.DataAccess.Data.Repository
{
    public class DocumentCatalogRepository : Repository<DocumentCatalog>, IDocumentCatalogRepository
    {
        private readonly ApplicationDbContext _db;

        public DocumentCatalogRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        //public IEnumerable<SelectListItem> GetDocumentCatalogList()
        //{
        //    return _db.DocumentCatalog.Select(i => new SelectListItem()
        //    {
        //        Text = i.Name,
        //        Value = i.Id.ToString()
        //    });
        //}
        public IEnumerable<DocumentVM> GetDocumentCatalogList()
        {
            return _db.DocumentCatalog.Select(doc => new DocumentVM
            {
                Id = doc.Id,
                Name = doc.Name,
                Description = doc.Description,
                Extension = doc.Extension
            }).ToList();
        }

        public void Update(DocumentCatalog documentCatalog)
        {
            var objDesdeDb = _db.DocumentCatalog.FirstOrDefault(s => s.Id == documentCatalog.Id);
            objDesdeDb.Name = documentCatalog.Name;
            objDesdeDb.Description = documentCatalog.Description;
            objDesdeDb.Extension = documentCatalog.Extension;

            _db.SaveChanges();
        }
    }
}
