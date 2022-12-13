using Microsoft.Extensions.Configuration;
using Models;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Repositories
{
    internal class DojoRepository
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private bool _isLoggedIn;
        public DojoRepository(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _serializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public async Task<StoryViewModel> GetStoriesAsync(string before = null, CancellationToken token = default)
        {
            return await ExecuteAsLoggedInUser(async () =>
            {
                var response = await _httpClient.GetAsync($"storyFeed?before={before}", token);

                response.EnsureSuccessStatusCode();

                using var responseStream = await response.Content.ReadAsStreamAsync();

                return await JsonSerializer.DeserializeAsync<StoryViewModel>(responseStream, _serializerOptions, token);
            }, token);
        }

        public async Task<Stream> GetImageStreamAsync(Uri uri, CancellationToken token = default)
        {
            return await ExecuteAsLoggedInUser(async () =>
            {
                var imageResponse = await _httpClient.GetAsync(uri, token);
                return await imageResponse.Content.ReadAsStreamAsync();
            }, token);
        }

        async Task<T> ExecuteAsLoggedInUser<T>(Func<Task<T>> f, CancellationToken token = default)
        {
            if (!_isLoggedIn)
            {
                var loginRequest = new
                {
                    login = _configuration["login"],
                    password = _configuration["password"]
                };
                var httpContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
                using var response = await _httpClient.PostAsync("session", httpContent, token);

                response.EnsureSuccessStatusCode();

                _isLoggedIn = true;
            }

            return await f();
        }
    }
}