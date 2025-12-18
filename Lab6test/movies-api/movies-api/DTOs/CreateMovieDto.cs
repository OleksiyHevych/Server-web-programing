namespace MoviesService.DTOs
{
    public class CreateMovieDto
    {
        public string Title { get; set; } = "";
        public string Genre { get; set; } = "";
        public int ReleaseYear { get; set; }
    }
}
