using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.Models.ViewModels
{
    public class TeamVM
    {
        public Team Team { get; set; }
        public IEnumerable<SelectListItem> CoachList { get; set; }
        public IEnumerable<SelectListItem> RepresentativeList { get; set; }
        public IEnumerable<SelectListItem> StudentList { get; set; }
    }
}
