namespace MovieActorsService.DTOs
{
    public class ActorInMovieDto
    {
        public int Id { get; set; }           // ActorId
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string CharacterName { get; set; } = "";
        public object Value { get; internal set; }
    }

}
