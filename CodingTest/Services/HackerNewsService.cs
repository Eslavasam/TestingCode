using System.Text.Json;
using CodingTest.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CodingTest.Services
{
    public class HackerNewsService : IHackerNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HackerNewsService> _logger;
        private readonly SemaphoreSlim _semaphore;

        public HackerNewsService(HttpClient httpClient, IMemoryCache cache, ILogger<HackerNewsService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _semaphore = new SemaphoreSlim(20);
        }

        public async Task<IEnumerable<Story>> GetBestStoriesAsync(int n)
        {
            if (n<=0)
                return Enumerable.Empty<Story>();

            var storyIds = await GetBestStoryIds();
            var stories = await GetStoriesDetails(storyIds);

            return stories.OrderByDescending(s => s.Score).Take(n);
        }

        private async Task<IEnumerable<int>> GetBestStoryIds()
        {
            const string cacheKey = "best_story_ids";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<int> storyIds))
            {
                try
                {
                    var response = await _httpClient.GetAsync("beststories.json");
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();
                    storyIds = JsonSerializer.Deserialize<IEnumerable<int>>(content);
                    _cache.Set(cacheKey, storyIds, TimeSpan.FromMinutes(5));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching best story Ids from Hacker Newa API");
                    throw;
                }
            }
            return storyIds;
        }

        private async Task<IEnumerable<Story>> GetStoriesDetails(IEnumerable<int>storyIds)
        {
            var task = storyIds.Select(id => GetStoryDetails(id));
            var stories = await Task.WhenAll(task);
            return stories.Where(story => story!=null);

        }

        private async Task<Story> GetStoryDetails(int storyId)
        {
            var cacheKey = $"{storyId}";
            if (!_cache.TryGetValue(cacheKey,out Story story))
            {
                try
                {
                    await _semaphore.WaitAsync();
                    var response = await _httpClient.GetAsync($"item/{storyId}.json");
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(content);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("type", out var type) && type.GetString() == "story")
                    {
                        story = new Story
                        {
                            Title = root.TryGetProperty("title", out var title) ? title.GetString() : null,
                            Url = root.TryGetProperty("url", out var url) ? url.GetString() : null,
                            PostedBy = root.TryGetProperty("by", out var by) ? by.GetString() : null,
                            Time = root.TryGetProperty("time", out var time) ? DateTimeOffset.FromUnixTimeSeconds(time.GetInt64()).DateTime : DateTime.MinValue,
                            Score = root.TryGetProperty("score", out var score) ? score.GetInt32() : 0,
                            CommentCount = root.TryGetProperty("descendants", out var descendants)?descendants.GetInt32():0
                        };

                        _cache.Set(cacheKey, story, TimeSpan.FromMinutes(5));
                    }
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex, "Error fetching story details for Id {StoryId}", storyId);
                    return null;
                }
                finally 
                {
                    _semaphore.Release();
                }
            }

            return story;

        }
    }
}
