namespace MovieActorsService.DTOs
{
    public class CreateMovieActorDto
    {
        public int MovieId { get; set; }
        public int ActorId { get; set; }
        public string CharacterName { get; set; } = "";
    }
}
