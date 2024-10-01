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
        [Display(Name="Prenda")]
        [Required(ErrorMessage = "Este campo es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        public string Name { get; set; }
        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "Este campo es obligatorio.")]
        [StringLength(100, ErrorMessage = "La descripción no puede acceder los 100 caracteres.")]
        public string Description { get; set; }
        public ICollection<Representative>? Representative { get; set; }
    }
}
