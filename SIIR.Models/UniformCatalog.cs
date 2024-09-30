using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.Models
{
    public class UniformCatalog
    {
        [Key]
        public int Id { get; set; }
        [Display(Name="Nombre")]
        [Required(ErrorMessage = "Este campo es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        public string Name { get; set; }
        [Display(Name = "¿Tiene número?")]
        [Required(ErrorMessage = "Este campo es obligatorio.")]
        public bool HasNumber { get; set; }
        [Required(ErrorMessage = "Debe seleccionar un grupo representativo.")]
        public int RepresentativeId { get; set; }
        [ForeignKey("RepresentativeId")]
        public Representative? Representative { get; set; }
    }
}
