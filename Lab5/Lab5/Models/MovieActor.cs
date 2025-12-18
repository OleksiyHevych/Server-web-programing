using System.ComponentModel.DataAnnotations;

namespace Lab5.Models
{
    public class MovieActor
    {
        [Required]
        public int MovieId { get; set; }
        public Movie? Movie { get; set; }

        [Required]
        public int ActorId { get; set; }
        public Actor? Actor { get; set; }

        [StringLength(100)]
        public string? RoleName { get; set; }

        [Range(1, 100)]
        public int BillingOrder { get; set; }
    }
}
