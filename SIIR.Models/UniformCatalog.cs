using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.Models
{
    public class UniformCatalog
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool? HasNumber { get; set; }

        public int? RepresentativeId { get; set; }
        [ForeignKey("RepresentativeId")]
        public Representative? Representative { get; set; }
    }
}
