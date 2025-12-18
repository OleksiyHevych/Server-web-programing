using ActorsApi.DTOs;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ActorsApi.Services;

public class MovieActorsLookup
{
    private readonly HttpClient _client;

    public MovieActorsLookup(HttpClient client)
    {
        _client = client;
    }

    public async Task<List<MovieDto>> GetMoviesForActorAsync(int actorId, string token)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/movie-actors/actor/{actorId}"
        );

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return new List<MovieDto>();

        var content = await response.Content.ReadAsStringAsync();

        var movieActors = JsonSerializer.Deserialize<List<MovieActorInfoDto>>(
            content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (movieActors == null)
            return new List<MovieDto>();

        return movieActors.Select(ma => new MovieDto
        {
            Id = ma.MovieId,
            Title = ma.MovieTitle,
            ReleaseYear = ma.MovieReleaseYear
        }).ToList();
    }

}
