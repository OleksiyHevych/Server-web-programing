namespace ActorsApi.DTOs;

// DTO для MovieActor
public class MovieActorInfoDto
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = "";
    public int MovieReleaseYear { get; set; }
}


// DTO для Movie
public class MovieDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int ReleaseYear { get; set; }
}
