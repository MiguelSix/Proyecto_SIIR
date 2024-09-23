using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.Models
{
    internal class ApplicationUser : IdentityUser
    {
        public string Nombre { get; set; }

        public string Apellido { get; set; }

        // Implementación de la relación con los roles (Admin, Student, Coach)

        public int? AdminId { get; set; }
        [ForeignKey("AdminId")]
        public Admin Admin { get; set; }

        public int? StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        public int? CoachId { get; set; }
        [ForeignKey("CoachId")]
        public Coach Coach { get; set; }
    }
}
