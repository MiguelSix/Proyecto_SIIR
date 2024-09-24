using System.ComponentModel.DataAnnotations;

namespace SIIR.Models
{
    public class Coach
    {
        [Key]
        public int Id { get; set; }

        /*public int? TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team Team { get; set; }*/
    }
}