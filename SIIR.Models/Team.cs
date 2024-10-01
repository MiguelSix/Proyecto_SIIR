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
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        public string Category { get; set; }

        [Display(Name = "Imagen")]
        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; }

        // Llaves foráneas

        //COACH ID
        [Required]
        public int? CoachId { get; set; }
        [ForeignKey("CoachId")]
        public Coach Coach { get; set; }
        //EQUIPO REPRESENTATIVO ID
        [Required]
        public int? RepresentativeId { get; set; }
        [ForeignKey("RepresentativeId")]
        public Representative Representative { get; set; }

        //CAPITAN ID
        public int? StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        //UNIFORM CATALOG
        public ICollection<UniformCatalog>? UniformCatalogs { get; set; }
    }
}
