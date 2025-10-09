namespace Lab1.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int DurationMinutes { get; set; }
        public string? Description { get; set; }

        // many-to-many
        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}