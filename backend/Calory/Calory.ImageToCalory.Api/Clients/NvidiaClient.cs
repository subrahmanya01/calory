using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Calory.ImageToCalory.Api.Clients
{
    public class NvidiaClient
    {
        private readonly HttpClient _httpClient;
        public NvidiaClient( HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<JsonDocument> ChatCompletionAsync(object payload, string apiKey, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage( HttpMethod.Post, "chat/completions");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Headers.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json"));

            var json = JsonSerializer.Serialize(payload);

            request.Content = new StringContent( json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync( request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync( cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"NVIDIA API failed. " + $"Status: {(int)response.StatusCode}. " + $"Response: {responseBody}");
            }

            return JsonDocument.Parse(responseBody);
        }
    }
}
