using System.Net.Http.Headers;
using System.Text.Json;
using MoviesService.DTOs;

namespace MoviesService.Services
{
    public class MovieActorsLookupClient
    {
        private readonly HttpClient _client;

        public MovieActorsLookupClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<ActorInMovieDto>> GetActorsForMovieAsync(int movieId, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movie-actors/movie/{movieId}");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return new List<ActorInMovieDto>();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ActorInMovieDto>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ActorInMovieDto>();
        }
    }
}
