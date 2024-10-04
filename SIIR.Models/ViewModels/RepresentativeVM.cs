using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.Models.ViewModels
{
    public class RepresentativeVM
    {
        public Representative Representative { get; set; }
        public IEnumerable<SelectListItem>? UniformCatalogList { get; set; }
        public List<int>? SelectedUniformCatalogIds { get; set; }
    }
}
