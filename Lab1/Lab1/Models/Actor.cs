namespace Lab1.Models
{
    public class Actor
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string? Country { get; set; }
        public string? Biography { get; set; }

        // many-to-many 
        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();

        public string Name => $"{FirstName} {LastName}";
    }
}