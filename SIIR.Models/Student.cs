using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIIR.Models
{
    public class Student
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


        [Required(ErrorMessage = "El número de control es obligatorio.")]
        [StringLength(8, ErrorMessage = "El número de control no puede exceder los 8 caracteres.")]
        [Display(Name = "Número de Control")]
        public string ControlNumber { get; set; }

        [StringLength(18, ErrorMessage = "El CURP no puede exceder los 18 caracteres.")]
        [Display(Name = "CURP")]
        public string? Curp { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        public string? BirthDate { get; set; }

        [Display(Name = "Carrera")]
        public string? Career { get; set; }

        [Range(1, 12, ErrorMessage = "El semestre debe estar entre 1 y 12.")]
        [Display(Name = "Semestre")]
        public string? Semester { get; set; }

        [Phone(ErrorMessage = "El número de teléfono no es válido.")]
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }

        [Display(Name = "Edad")]
        public int Age { get; set; }

        [StringLength(3, ErrorMessage = "El tipo de sangre no puede exceder los 3 caracteres.")]
        [Display(Name = "Tipo de Sangre")]
        public string? BloodType { get; set; }

        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [Display(Name = "Correo Electrónico")]
        public string? Email { get; set; }

        [Display(Name = "Peso")]
        public float? Weight { get; set; }

        [Display(Name = "Altura")]
        public float? Height { get; set; }

        [StringLength(250, ErrorMessage = "Las alergias no pueden exceder los 250 caracteres.")]
        [Display(Name = "Alergias")]
        public string? Allergies { get; set; }

        [StringLength(10, ErrorMessage = "El NSS no puede exceder los 15 caracteres.")]
        [Display(Name = "NSS")]
        public string? Nss { get; set; }

        // Llaves foráneas
        public int CoachId { get; set; }

        [ForeignKey("CoachId")]
        public Coach Coach { get; set; }

        // Equipo
        /*public int? TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team Team { get; set; }*/
    }
}
