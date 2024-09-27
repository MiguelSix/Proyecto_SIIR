using System.ComponentModel.DataAnnotations;

namespace SIIR.Models
{
    public class Representative
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}