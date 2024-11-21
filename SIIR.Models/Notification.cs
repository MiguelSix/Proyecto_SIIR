using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        public int? DocumentId {  get; set; }
        [ForeignKey("DocumentId")]
        public Document Document { get; set; }
        [Required]
        public String Message {  get; set; }
        [Required]
        [StringLength(50)]
        public string Type { get; set; }
        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
