using System;

namespace SimpleBrowser.Models
{
    public class HistoryItemModel
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DisplayTitle => Title.Length > 30 ? $"{Title[..30]}..." : Title;
        public DateTime DateVisited { get; set; } = DateTime.MinValue;
    }
}
