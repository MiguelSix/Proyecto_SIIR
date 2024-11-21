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

        public IEnumerable<Document> GetDocumentsByStudent(int studentId)
        {
            return _db.Document
                .Include(d => d.DocumentCatalog)
                .Where(d => d.StudentId == studentId)
                .ToList();
        }

        public Document GetDocumentWithCatalog(int id)
        {
            return _db.Document
                .Include(d => d.DocumentCatalog)
                .Include(d => d.Student)
                .FirstOrDefault(d => d.Id == id);
        }

        public void Update(Document document)
        {
            var objDesdeDb = _db.Document.FirstOrDefault(s => s.Id == document.Id);
            if (objDesdeDb != null)
            {
                objDesdeDb.StudentId = document.StudentId;
                objDesdeDb.DocumentCatalogId = document.DocumentCatalogId;
                objDesdeDb.UploadDate = document.UploadDate;
                objDesdeDb.Url = document.Url;
                objDesdeDb.Status = document.Status;
                objDesdeDb.RejectionReason = document.RejectionReason;
            }
        }

    }
}
