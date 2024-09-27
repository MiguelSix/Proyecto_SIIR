using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.Models
{
    public class Team
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Category { get; set; }

        // Llaves foráneas

        //COACH ID
        [Required]
        public int CoachId { get; set; }
        [ForeignKey("CoachId")]
        public Coach Coach { get; set; }
        //EQUIPO REPRESENTATIVO ID
        [Required]
        public int RepresentativeId { get; set; }
        [ForeignKey("RepresentativeId")]
        public Representative Representative { get; set; }

        //CAPITAN ID
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; }
    }
}
