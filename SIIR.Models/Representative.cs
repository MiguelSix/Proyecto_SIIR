using System.ComponentModel.DataAnnotations;

namespace SIIR.Models
{
    public class Representative
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es del grupo Representativo es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        [Display(Name = "Nombre del grupo Representativo.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "La categoría del grupo Representativo es obligatoria.")]
        [Display(Name = "Categoría del grupo Representativo.")]
        public string Category { get; set; }
     }
}