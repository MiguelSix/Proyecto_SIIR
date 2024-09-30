using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.Models.ViewModels
{
    public class UniformCatalogVM
    {
        public UniformCatalog UniformCatalog { get; set; }
        public IEnumerable<SelectListItem>? RepresentativeList { get; set; }
    }
}
