using System.ComponentModel.DataAnnotations;

namespace SIIR.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        [Display(Name = "Nombre")]
        public string? Name { get; set; }

        [StringLength(50, ErrorMessage = "El apellido paterno no puede exceder los 50 caracteres.")]
        [Display(Name = "Apellido Paterno")]
        public string? LastName { get; set; }

        [StringLength(50, ErrorMessage = "El apellido materno no puede exceder los 50 caracteres.")]
        [Display(Name = "Apellido Materno")]
        public string? SecondLastName { get; set; }

    }
}