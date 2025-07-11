using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using tech_news_app_server.NewsApi;
using System;

namespace tech_news_app_server.NewsApi
{
    public class NewsApiSettings
    {
        public required string ApiKey { get; set; }
        public required string BaseUrl { get; set; }
    }

    public class Source
    {
        public string? id { get; set; }
        public string? name { get; set; }
    }
    public class Article
    {
        public Source? source { get; set; }
        public string? author { get; set; }
        public required string title { get; set; }
        public required string description { get; set; }
        public required string url { get; set; }
        public required string urlToImage { get; set; }
        public required string publishedAt { get; set; }
        public string? content { get; set; }
 
    }

    public class NewsApiResponse
    {
        public required string status { get; set; }
        public required int totalResults { get; set; }
        public required List<Article> Articles { get; set; }
    }

    public class CategoryRequest
    {
        public string category { get; set; } = string.Empty;
        public string country { get; set; } = string.Empty;
    }
    public class GenreRequest
    {
        public string genre { get; set; } = string.Empty;
        public string country { get; set; } = string.Empty;
    }

}

namespace tech_news_app_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewsApiController : ControllerBase
    {

        private readonly NewsApiSettings _newsApiSettings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<NewsApiController> _logger;

        public NewsApiController(IOptions<NewsApiSettings> settings, IHttpClientFactory httpClientFactory, ILogger<NewsApiController> logger)
        {
            _newsApiSettings = settings.Value;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        // Task to fetch articles from the News API, responds with a JSON string object
        private async Task<NewsApiResponse> GetArticleContent(HttpClient httpClient, string url)
        {
            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(responseContent))
                {
                    throw new HttpRequestException("Received empty response from the news API.");
                }

                var NewsResponseObject = JsonSerializer.Deserialize<NewsApiResponse>(responseContent);
                if (NewsResponseObject != null)
                {
                    return NewsResponseObject;
                }
                else
                {
                    throw new Exception("Failed to deserialize response from News API into JSON object...");
                }                
            } 
            catch (HttpRequestException e)
            {
                throw new HttpRequestException("Error contacting News API:", e);
            }
        }

        // fetch data from news API using category (Most likely 'technology')
        [HttpPost("bycategory")]
        public async Task<IActionResult> GetArticlesByCategory([FromBody] CategoryRequest request)
        {
            if (string.IsNullOrEmpty(request.category))
            {
                return BadRequest("Category cannot be null or empty.");
            }
            int page = 1;
            int totalProcessed = 0;
            int totalResults = int.MaxValue;
            var articles = new List<Article>();
            string _url = string.Empty;
            while (totalProcessed < totalResults)
            {
                try
                {
                    _url = $"{_newsApiSettings.BaseUrl}/top-headlines?category={request.category}&country={request.country}&apiKey={_newsApiSettings.ApiKey}&page={page}";
                    var NewsObject = await GetArticleContent(_httpClient, _url);
                    if (NewsObject != null)
                    {
                        totalResults = NewsObject.totalResults;
                        totalProcessed += NewsObject.Articles.Count;
                        articles.AddRange(NewsObject.Articles);
                        page++;
                    }
                    else
                    {
                        throw new Exception("Response from GetArticleContent returned null, terminating...");
                    }
                }
                catch (HttpRequestException e)
                {
                    throw new HttpRequestException($"Error fetching category: {request.category}, resulting in: {e}");
                }
            }
            return Ok(articles);
        }

        // fetch data from News API using genre keyword by query
        [HttpPost("bygenre")]
        public async Task<IActionResult> GetArticlesByGenre([FromBody] GenreRequest request)
        {
            if (string.IsNullOrEmpty(request.genre))
            {
                return BadRequest("Category cannot be null or empty.");
            }
            int page = 1;
            int totalProcessed = 0;
            int totalResults = int.MaxValue;
            var articles = new List<Article>();
            while (totalProcessed < totalResults)
            {
                try
                {
                    string _url = $"{_newsApiSettings.BaseUrl}/top-headlines?q={request.genre}&country={request.country}&apiKey={_newsApiSettings.ApiKey}&page={page}";
                    var NewsObject = await GetArticleContent(_httpClient, _url);
                    if (NewsObject != null)
                    {
                        totalResults = NewsObject.totalResults;
                        totalProcessed += NewsObject.Articles.Count;
                        articles.AddRange(NewsObject.Articles);
                        page++;
                    }
                    else
                    {
                        throw new System.Exception("Response from GetArticleContent returned null, terminating...");
                    }
                }
                catch (HttpRequestException e)
                {
                    throw new HttpRequestException($"Error fetching genre: {request.genre}, resulting in: {e}");
                }
            }
            return Ok(articles);
        }
    }
}
