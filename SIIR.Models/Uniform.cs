using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIIR.Models
{
    public enum Size
    {
        CH,
        M,
        L,
        XL,
        XXL
    }

    public class Uniform
    {
        [Key]
        public int Id { get; set; }
        public Size? size { get; set; }  
        public int? number { get; set; }
        public int StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; }

		// Clave primaria compuesta para la relación con RepresentativeUniformCatalog

		public int RepresentativeId { get; set; }
		public int UniformCatalogId { get; set; }

		[ForeignKey("RepresentativeId, UniformCatalogId")]
		public RepresentativeUniformCatalog? RepresentativeUniformCatalog { get; set; }
	}
}
