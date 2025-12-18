using MovieActorsService.DTOs;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MovieActorsService.Services;

public class MoviesClient
{
    private readonly HttpClient _client;

    public MoviesClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<MovieDto?> GetMovieAsync(int movieId, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/movies/{movieId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        return JsonSerializer.Deserialize<MovieDto>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<bool> MovieExists(int movieId, string token)
        => await GetMovieAsync(movieId, token) != null;
}
