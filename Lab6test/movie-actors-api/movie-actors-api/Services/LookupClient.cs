using MovieActorsService.DTOs;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MovieActorsService.Services
{
    public class LookupClient
    {
        private readonly HttpClient _client;

        public LookupClient(HttpClient client)
        {
            _client = client;
        }

        // Отримати дані фільму
        public async Task<MovieDto?> GetMovieAsync(int movieId, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/movies/{movieId}");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MovieDto>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<bool> ActorExists(int actorId, string token)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/actors/{actorId}"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);

            Console.WriteLine(
                $"[ActorExists] GET /api/actors/{actorId} -> {(int)response.StatusCode} {response.StatusCode}"
            );

            return response.IsSuccessStatusCode;
        }


        // Перевірка існування фільму
        public async Task<bool> MovieExists(int movieId, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/movies/{movieId}");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // Отримати повного актора
        public async Task<ActorInMovieDto?> GetActorAsync(int actorId, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/actors/{actorId}");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ActorInMovieDto>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
