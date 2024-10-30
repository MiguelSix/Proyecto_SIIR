using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIIR.Models
{
    public class RepresentativeUniformCatalog
    {
        [Key, Column(Order = 1)]
        public int RepresentativeId { get; set; }

        [ForeignKey(nameof(RepresentativeId))]
        public Representative Representative { get; set; }

        [Key, Column(Order = 2)]
        public int UniformCatalogId { get; set; }

        [ForeignKey(nameof(UniformCatalogId))]
        public UniformCatalog UniformCatalog { get; set; }
    }
}
