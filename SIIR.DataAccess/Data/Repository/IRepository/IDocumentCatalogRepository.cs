using Microsoft.AspNetCore.Mvc.Rendering;
using SIIR.Models;
using SIIR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.DataAccess.Data.Repository.IRepository
{
    public interface IDocumentCatalogRepository : IRepository<DocumentCatalog>
    {
        IEnumerable<SelectListItem> GetDocumentCatalogList();
        void Update(DocumentCatalog documentCatalog);
        DocumentCatalog GetById(int id);
    }
}
