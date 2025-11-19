using System.ComponentModel.DataAnnotations;

namespace Lab1.Models
{
    public class MovieActor
    {
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        [Required]
        public int ActorId { get; set; }
        public Actor Actor { get; set; }

        [StringLength(100, ErrorMessage = "Назва ролі не може перевищувати 100 символів")]
        public string? RoleName { get; set; }

        [Range(1, 100, ErrorMessage = "Порядок у титрах має бути між 1 і 100")]
        public int BillingOrder { get; set; }
    }
}
