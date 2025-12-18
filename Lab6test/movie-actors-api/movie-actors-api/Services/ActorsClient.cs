using MovieActorsService.DTOs;
using System.Net.Http.Headers;
using System.Text.Json;


namespace MovieActorsService.Services;

public class ActorsClient
{
    private readonly HttpClient _client;

    public ActorsClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<ActorInMovieDto?> GetActorAsync(int actorId, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/actors/{actorId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        return JsonSerializer.Deserialize<ActorInMovieDto>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<bool> ActorExists(int actorId, string token)
        => await GetActorAsync(actorId, token) != null;
}
