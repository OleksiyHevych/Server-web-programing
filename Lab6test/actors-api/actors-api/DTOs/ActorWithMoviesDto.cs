namespace ActorsApi.DTOs;

public class ActorWithMoviesDto
{
	public int Id { get; set; }
	public string FirstName { get; set; } = null!;
	public string LastName { get; set; } = null!;
	public DateTime BirthDate { get; set; }
	public string Country { get; set; } = null!;
	public string Biography { get; set; } = null!;

	public List<MovieDto> Movies { get; set; } = new(); // “ут п≥дт€гуютьс€ ф≥льми через MovieActorsService
}
