namespace Lab5.Models
{
    public class ActorDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string? Country { get; set; }
        public string? Biography { get; set; }

        public List<ActorMovieDto> Movies { get; set; } = new();
    }

    public class ActorMovieDto
    {
        public int MovieId { get; set; }
        public string? Title { get; set; }
        public string? RoleName { get; set; }
    }

    public class MovieDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int DurationMinutes { get; set; }
        public string? Description { get; set; }

        public List<MovieActorDto> Actors { get; set; } = new();
    }

    public class MovieActorDto
    {
        public int ActorId { get; set; }
        public string? Name { get; set; }
        public string? RoleName { get; set; }
    }
}

