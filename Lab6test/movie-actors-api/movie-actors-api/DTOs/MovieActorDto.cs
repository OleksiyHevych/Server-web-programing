namespace MovieActorsService.DTOs
{
    public class MovieActorDto
    {
        public int MovieId { get; set; }
        public string ActorFullName { get; set; } = "";
        public string MovieTitle { get; set; } = "";
        public string MovieGenre { get; set; } = "";       // нове поле
        public int MovieReleaseYear { get; set; }           // тепер int
        public int ActorId { get; set; }
        public string CharacterName { get; set; } = "";
    }
}
