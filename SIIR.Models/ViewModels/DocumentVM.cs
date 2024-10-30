using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.Models.ViewModels
{
    public class DocumentVM
    {
        public Document Document { get; set; }
        public IEnumerable<SelectListItem> ListDocumenCatalog { get; set; }
        
        // Para mostrar los documentos actuales del estudiante
        public IEnumerable<Document> StudentDocuments { get; set; }


    }
}
