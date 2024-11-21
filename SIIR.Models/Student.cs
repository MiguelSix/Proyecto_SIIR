using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIIR.Models
{
    public class Student
    {
        // Campos Comunes de los 3 Roles

        [Key]
        public int Id { get; set; }

        //[Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        [Display(Name = "Nombre")]
        public string? Name { get; set; }

        //[Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido paterno no puede exceder los 50 caracteres.")]
        [Display(Name = "Apellido Paterno")]
        public string? LastName { get; set; }

        //[Required(ErrorMessage = "El apellido materno es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido materno no puede exceder los 50 caracteres.")]
        [Display(Name = "Apellido Materno")]
        public string? SecondLastName { get; set; }

        // Campos Comunes de los 3 Roles


        [StringLength(8, ErrorMessage = "El número de control no puede exceder los 8 caracteres.")]
        [Display(Name = "Número de Control")]
        public string? ControlNumber { get; set; }

        [StringLength(18, ErrorMessage = "El CURP no puede exceder los 18 caracteres.")]
        [Display(Name = "CURP")]
        public string? Curp { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        public string? BirthDate { get; set; }

        [Display(Name = "Carrera")]
        public string? Career { get; set; }

        [Display(Name = "Semestre")]
        public string? Semester { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe contener 10 dígitos")]
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }

        [Display(Name = "Edad")]
        public int? Age { get; set; }

        [StringLength(3, ErrorMessage = "El tipo de sangre no puede exceder los 3 caracteres.")]
        [Display(Name = "Tipo de Sangre")]
        public string? BloodType { get; set; }

        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [Display(Name = "Correo Electrónico")]
        public string? Email { get; set; }

		[Display(Name = "Peso")]
		[StringLength(6, ErrorMessage = "El peso no puede tener más de 6 caracteres.")]
		[RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Formato incorrecto.")]
		public string? Weight { get; set; }

		[Display(Name = "Altura")]
		[StringLength(4, ErrorMessage = "La altura no puede tener más de 4 caracteres.")]
		[RegularExpression(@"^\d(\.\d{1,2})?$", ErrorMessage = "Formato incorrecto.")]
		public string? Height { get; set; }

		[StringLength(250, ErrorMessage = "Las alergias no pueden exceder los 250 caracteres.")]
        [Display(Name = "Alergias")]
        public string? Allergies { get; set; }

        [StringLength(11, ErrorMessage = "El NSS no puede exceder los 15 caracteres.")]
        [Display(Name = "NSS")]
        public string? Nss { get; set; }

        [Display(Name = "Foto")]
        [DataType(DataType.ImageUrl)]
        public string? ImageUrl { get; set; }

        [Display(Name = "Número del jugador")]
        public int? numberUniform { get; set; }

        [Display(Name = "Fecha de ingreso")]
		public int? enrollmentData { get; set; }

        [Display(Name = "Nombre del Contacto de emergencia")]
        [StringLength(50, ErrorMessage = "El nombre del contacto de emergencia no puede exceder los 50 caracteres.")]
        public string? emergencyContact { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "El número de teléfono de emergencia no es válido.")]
        [Display(Name = "Teléfono de emergencia")]
        public string? emergencyPhone { get; set; }

        // Llaves foráneas
        public int? CoachId { get; set; }

        [ForeignKey("CoachId")]
        public Coach? Coach { get; set; }

        // Equipo
        public int? TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team? Team { get; set; }

        // Capitán
        [DefaultValue(false)]
        public bool IsCaptain { get; set; }
	}
}
