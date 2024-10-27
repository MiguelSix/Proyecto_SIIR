using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.Models.ViewModels
{
    public class StudentVM
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string SecondLastName { get; set; }
        public string ControlNumber { get; set; }
        public string Career { get; set; }
        public string Semester { get; set; }
        public string ImageUrl { get; set; }
        public Student Student { get; set; }

        public string FullName => $"{Name} {LastName} {SecondLastName}".Trim();
        public IEnumerable<SelectListItem>? TeamList { get; set; }
    }
}
