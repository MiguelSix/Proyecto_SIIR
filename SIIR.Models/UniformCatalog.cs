using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        /*public int? RepresentativesId { get; set; }
        [ForeignKey("RepresentativesId")]
        public Representative Representative { get; set; }*/
    }
}
