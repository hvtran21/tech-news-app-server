using System;

namespace tech_news_app_server.Models
{
    public class Article
    {
        public required string Id {  get; set; }
        public required string Genre { get; set; }
        public required string Category { get; set; }
        public required string Source { get; set; }
        public required string Author { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Url { get; set; }
        public required string UrlToImage { get; set; }
        public required DateTime PublishedAt { get; set; }
        public required string Content { get; set; }

    }

}

