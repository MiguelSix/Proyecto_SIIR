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

        public IEnumerable<SelectListItem> ListaDocumenCatalog { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Extension { get; set; }
    }
}
