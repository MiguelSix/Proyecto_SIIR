using System.ComponentModel.DataAnnotations;

namespace SIIR.Models
{
    public class Coach
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido paterno no puede exceder los 50 caracteres.")]
        [Display(Name = "Apellido Paterno")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El apellido materno es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido materno no puede exceder los 50 caracteres.")]
        [Display(Name = "Apellido Materno")]
        public string SecondLastName { get; set; }

        /*public int? TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team Team { get; set; }*/
    }
}