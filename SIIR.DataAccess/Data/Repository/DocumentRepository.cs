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
    public class DocumentRepository : Repository<Document>, IDocumentRepository
    {
        private readonly ApplicationDbContext _db;

        public DocumentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public IEnumerable<SelectListItem> GetDocumentList()
        {
            return _db.Document.Select(i => new SelectListItem()
            {
                Value = i.Id.ToString()
            });
        }

        public void Update(Document document)
        {
            var objDesdeDb = _db.Document.FirstOrDefault(s => s.Id == document.Id);
            objDesdeDb.StudentId = document.StudentId;
            objDesdeDb.DocumentCatalogId = document.DocumentCatalogId;
            objDesdeDb.UploadDate = document.UploadDate;
            objDesdeDb.Url = document.Url;

            _db.SaveChanges();
        }

    }
}
