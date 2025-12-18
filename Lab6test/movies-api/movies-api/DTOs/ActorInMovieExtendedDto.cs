namespace MoviesService.DTOs
{
    public class ActorInMovieExtendedDto
    {
        public int MovieId { get; set; }
        public int ActorId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string CharacterName { get; set; } = "";
    }
}
