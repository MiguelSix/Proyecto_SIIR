using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.Models
{
    public class DocumentCatalog
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre del documento")]
        [StringLength(100, ErrorMessage = "El nombre del documento no puede exceder los 100 caracteres")]
        [Remote(action: "VerificarNombreUnico", controller: "DocumentCatalog", ErrorMessage = "Ya existe un documento con este nombre.")]
        [Display(Name = "Nombre del Documento")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Debes seleccionar un tipo de documento.")]
        [RegularExpression("pdf|foto", ErrorMessage = "Debes seleccionar un tipo de documento válido.")]
        [Display(Name = "Tipo de docuemento")]
        public string Extension { get; set; }

        [StringLength(250, ErrorMessage = "La descripción no puede exceder los 250 caracteres")]
        [Display(Name = "Descripción del Documento")]
        public string? Description { get; set; }
    }
}
