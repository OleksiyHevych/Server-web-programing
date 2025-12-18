namespace MovieActorsService.Models
{
    public class MovieActor
    {
        public int MovieId { get; set; }
        public int ActorId { get; set; }
        public string CharacterName { get; set; } = "";
        public DateTime JoinedAt { get; set; }
    }
}
