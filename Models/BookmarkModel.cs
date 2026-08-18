using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBrowser.Models
{
    public class BookmarkModel
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName => Name.Length > 30 ? $"{Name[..30]}..." : Name;
    }
}
