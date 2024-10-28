using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }
        public int StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; }
        public int DocumentCatalogId { get; set; }
        [ForeignKey(nameof(DocumentCatalogId))]
        public DocumentCatalog DocumentCatalog { get; set; }
        [Display(Name = "Fecha de Subida")]
        public string UploadDate { get; set; }
        
        public string Url{ get; set; }

    }
}
